using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using ServiceStack.OrmLite;
using Storm.Api.CQRS.Exceptions;
using Storm.Api.Databases.Configurations.HighAvailability;
using Storm.Api.Logs;
using Storm.Api.Logs.Extensions;

namespace Storm.Api.Databases.Connections.HighAvailability;

internal sealed class DatabaseReplicaManager : IDatabaseReplicaManager
{
	private readonly HighAvailabilityOptions _options;
	private readonly ILogService? _logService;
	private readonly List<ReplicaSlot> _slots;
	private readonly Lock _stateLock = new();
	private int _roundRobin = -1;
	private ReplicaStatus? _primarySnapshot;

	public DatabaseReplicaManager(
		IEnumerable<ReplicaDefinition> replicas,
		IOrmLiteDialectProvider dialectProvider,
		HighAvailabilityOptions options,
		ILogService? logService)
	{
		_options = options;
		_logService = logService;
		_slots = replicas
			.Select(def => new ReplicaSlot(def, dialectProvider, options.ProbeConnectTimeout))
			.ToList();
		if (_slots.Count == 0)
		{
			throw new InvalidOperationException("HA manager requires at least one replica definition.");
		}
	}

	public bool IsPrimaryAvailable
	{
		get
		{
			lock (_stateLock)
			{
				return _slots.Any(s => s.Status is { State: ReplicaState.Online, Role: ReplicaRole.Primary });
			}
		}
	}

	public void EnsurePrimaryAvailable()
	{
		if (!IsPrimaryAvailable)
		{
			throw DomainDatabaseException.PrimaryUnavailable();
		}
	}

	public IReadOnlyList<ReplicaStatus> GetReplicaStates()
	{
		lock (_stateLock)
		{
			return _slots.Select(s => s.Status).ToList();
		}
	}

	public async Task<IDbConnection> OpenWriteAsync(CancellationToken ct = default)
	{
		ReplicaSlot? slot = FindPrimary();
		if (slot is null)
		{
			await ProbeAllAsync(ct);
			slot = FindPrimary();
		}

		if (slot is null)
		{
			throw DomainDatabaseException.PrimaryUnavailable();
		}

		return await slot.OpenWorkConnection(ct);
	}

	public async Task<IDbConnection> OpenReadAsync(CancellationToken ct = default)
	{
		ReplicaSlot? slot = FindReadEligibleSecondary();
		if (slot is not null)
		{
			return await slot.OpenWorkConnection(ct);
		}

		if (_options.AllowReadFallbackToPrimary)
		{
			return await OpenWriteAsync(ct);
		}

		throw new DomainDatabaseException(
			DomainDatabaseException.PrimaryUnavailableErrorCode,
			"No read-eligible replica available and fallback to primary is disabled.");
	}

	public async Task ProbeAllAsync(CancellationToken ct = default)
	{
		List<(ReplicaStatus previous, ReplicaStatus next)> transitions = new();
		foreach (ReplicaSlot slot in _slots)
		{
			ReplicaStatus previous = slot.Status;
			ReplicaStatus next = await ProbeSlotAsync(slot, ct);
			slot.Status = next;
			if (!StatusEquals(previous, next))
			{
				transitions.Add((previous, next));
			}
		}

		FireCallbacks(transitions);
	}

	public async Task PromoteToPrimaryAsync(string host, string availabilityGroupName, bool allowDataLoss = false, CancellationToken ct = default)
	{
		ReplicaSlot? target = _slots.FirstOrDefault(s => string.Equals(s.Definition.Host, host, StringComparison.OrdinalIgnoreCase));
		if (target is null)
		{
			throw new InvalidOperationException($"No configured replica with host '{host}'.");
		}

		string sql = SqlServerHaCommands.BuildForceFailoverSql(availabilityGroupName, allowDataLoss);
		using (IDbConnection db = await target.OpenWorkConnection(ct))
		{
			await ExecuteNonQueryAsync(db, sql, ct);
		}

		await ProbeAllAsync(ct);
	}

	private ReplicaSlot? FindPrimary()
	{
		lock (_stateLock)
		{
			return _slots.FirstOrDefault(s => s.Status is { State: ReplicaState.Online, Role: ReplicaRole.Primary });
		}
	}

	private ReplicaSlot? FindReadEligibleSecondary()
	{
		lock (_stateLock)
		{
			List<ReplicaSlot> eligible = _slots
				.Where(s => s.Status is { State: ReplicaState.Online, Role: ReplicaRole.Secondary, IsReadEligible: true })
				.ToList();
			if (eligible.Count == 0)
			{
				return null;
			}

			int index = Interlocked.Increment(ref _roundRobin) & int.MaxValue;
			return eligible[index % eligible.Count];
		}
	}

	private async Task<ReplicaStatus> ProbeSlotAsync(ReplicaSlot slot, CancellationToken ct)
	{
		ReplicaStatus previous = slot.Status;
		try
		{
			using IDbConnection db = await slot.OpenProbeConnection(ct);
			string? updateability = await ScalarStringAsync(db, SqlServerHaCommands.DetectUpdateabilitySql, ct);
			ReplicaRole role = updateability switch
			{
				"READ_WRITE" => ReplicaRole.Primary,
				"READ_ONLY" => ReplicaRole.Secondary,
				_ => ReplicaRole.Unknown,
			};

			bool isReadEligible = role == ReplicaRole.Primary;
			if (role == ReplicaRole.Secondary)
			{
				isReadEligible = await IsSecondaryReadEligibleAsync(db, ct);
			}

			ReplicaRole lastKnownRole = role == ReplicaRole.Unknown ? previous.LastKnownRole : role;
			return new ReplicaStatus(
				slot.Definition.Host,
				slot.Definition.Port,
				ReplicaState.Online,
				role,
				lastKnownRole,
				DateTimeOffset.UtcNow,
				null,
				isReadEligible);
		}
		catch (Exception ex)
		{
			return new ReplicaStatus(
				slot.Definition.Host,
				slot.Definition.Port,
				ReplicaState.Offline,
				ReplicaRole.Unknown,
				previous.LastKnownRole,
				DateTimeOffset.UtcNow,
				ex.Message,
				false);
		}
	}

	private async Task<bool> IsSecondaryReadEligibleAsync(IDbConnection db, CancellationToken ct)
	{
		if (_options.AllowReadsOnAsyncReplicas)
		{
			return true;
		}

		try
		{
			using IDbCommand cmd = db.CreateCommand();
			cmd.CommandText = SqlServerHaCommands.DetectSynchronousCommitSql;
			if (cmd is DbCommand dbCmd)
			{
				using DbDataReader reader = await dbCmd.ExecuteReaderAsync(ct);
				if (await reader.ReadAsync(ct))
				{
					string? syncState = reader.IsDBNull(0) ? null : reader.GetString(0);
					string? mode = reader.IsDBNull(1) ? null : reader.GetString(1);
					return syncState == "SYNCHRONIZED" && mode == "SYNCHRONOUS_COMMIT";
				}
			}

			return false;
		}
		catch
		{
			// Lack of VIEW SERVER STATE, transient error, etc. Treat as not eligible for reads.
			return false;
		}
	}

	private static async Task<string?> ScalarStringAsync(IDbConnection db, string sql, CancellationToken ct)
	{
		using IDbCommand cmd = db.CreateCommand();
		cmd.CommandText = sql;
		if (cmd is DbCommand dbCmd)
		{
			object? value = await dbCmd.ExecuteScalarAsync(ct);
			return value as string;
		}

		return cmd.ExecuteScalar() as string;
	}

	private static async Task ExecuteNonQueryAsync(IDbConnection db, string sql, CancellationToken ct)
	{
		using IDbCommand cmd = db.CreateCommand();
		cmd.CommandText = sql;
		if (cmd is DbCommand dbCmd)
		{
			await dbCmd.ExecuteNonQueryAsync(ct);
			return;
		}

		cmd.ExecuteNonQuery();
	}

	private void FireCallbacks(List<(ReplicaStatus previous, ReplicaStatus next)> transitions)
	{
		ReplicaStatus? previousPrimary;
		ReplicaStatus? currentPrimary;
		lock (_stateLock)
		{
			previousPrimary = _primarySnapshot;
			currentPrimary = _slots
				.Where(s => s.Status is { State: ReplicaState.Online, Role: ReplicaRole.Primary })
				.Select(s => s.Status)
				.FirstOrDefault();
			_primarySnapshot = currentPrimary;
		}

		foreach ((ReplicaStatus previous, ReplicaStatus next) in transitions)
		{
			if (_options.ReplicaStateChanged is { } replicaCb)
			{
				SafeInvoke(() => replicaCb(previous, next));
			}
		}

		if (previousPrimary is not null && currentPrimary is null && _options.PrimaryUnavailable is { } unavailableCb)
		{
			SafeInvoke(() => unavailableCb(previousPrimary));
		}

		if (previousPrimary is null && currentPrimary is not null && _options.PrimaryRestored is { } restoredCb)
		{
			SafeInvoke(() => restoredCb(currentPrimary));
		}

		if (previousPrimary is not null
			&& currentPrimary is not null
			&& (!string.Equals(previousPrimary.Host, currentPrimary.Host, StringComparison.OrdinalIgnoreCase)
				|| previousPrimary.Port != currentPrimary.Port)
			&& _options.PrimarySwitched is { } switchedCb)
		{
			SafeInvoke(() => switchedCb(previousPrimary, currentPrimary));
		}
	}

	private void SafeInvoke(Action callback)
	{
		try
		{
			callback();
		}
		catch (Exception ex)
		{
			_logService?.Error(x => x
				.WriteMessage("High-availability callback threw")
				.WriteException(ex));
		}
	}

	private static bool StatusEquals(ReplicaStatus a, ReplicaStatus b)
		=> a.State == b.State
			&& a.Role == b.Role
			&& a.IsReadEligible == b.IsReadEligible;

	private sealed class ReplicaSlot
	{
		private readonly IOrmLiteDialectProvider _dialect;
		private readonly string _probeConnectionString;
		private OrmLiteConnectionFactory? _workFactory;
		private OrmLiteConnectionFactory? _probeFactory;

		public ReplicaDefinition Definition { get; }
		public ReplicaStatus Status { get; set; }

		public ReplicaSlot(ReplicaDefinition def, IOrmLiteDialectProvider dialect, TimeSpan probeTimeout)
		{
			Definition = def;
			_dialect = dialect;
			_probeConnectionString = AdjustConnectTimeout(def.ConnectionString, Math.Max(1, (int)probeTimeout.TotalSeconds));
			Status = new ReplicaStatus(
				def.Host,
				def.Port,
				ReplicaState.Offline,
				ReplicaRole.Unknown,
				ReplicaRole.Unknown,
				DateTimeOffset.MinValue,
				null,
				false);
		}

		public Task<IDbConnection> OpenWorkConnection(CancellationToken ct)
		{
			_workFactory ??= new OrmLiteConnectionFactory(Definition.ConnectionString, _dialect);
			return _workFactory.OpenAsync(ct);
		}

		public Task<IDbConnection> OpenProbeConnection(CancellationToken ct)
		{
			_probeFactory ??= new OrmLiteConnectionFactory(_probeConnectionString, _dialect);
			return _probeFactory.OpenAsync(ct);
		}

		private static string AdjustConnectTimeout(string connectionString, int seconds)
		{
			SqlConnectionStringBuilder builder = new(connectionString)
			{
				ConnectTimeout = seconds,
			};
			return builder.ConnectionString;
		}
	}
}
