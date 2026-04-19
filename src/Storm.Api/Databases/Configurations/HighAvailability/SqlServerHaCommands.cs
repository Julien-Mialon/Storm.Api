using System.Text.RegularExpressions;

namespace Storm.Api.Databases.Configurations.HighAvailability;

/// <summary>
/// SQL helpers for SQL Server Always-On Availability Groups.
/// These are plain strings — callers decide when and where to execute them.
/// </summary>
public static partial class SqlServerHaCommands
{
	/// <summary>
	/// Parameter-less role probe. Returns <c>READ_WRITE</c> on the primary, <c>READ_ONLY</c> on
	/// a readable secondary, and other values (or an error) otherwise. Works without
	/// <c>VIEW SERVER STATE</c> permission.
	/// </summary>
	public const string DetectUpdateabilitySql =
		"SELECT CAST(DATABASEPROPERTYEX(DB_NAME(), 'Updateability') AS NVARCHAR(16));";

	/// <summary>
	/// Optional synchronous-commit check, used only when <c>AllowReadsOnAsyncReplicas</c> is false.
	/// Returns a single row with <c>synchronization_state_desc</c> and <c>availability_mode_desc</c>.
	/// Requires <c>VIEW SERVER STATE</c> on the replica.
	/// </summary>
	public const string DetectSynchronousCommitSql =
		"SELECT ars.synchronization_state_desc, ar.availability_mode_desc " +
		"FROM sys.dm_hadr_database_replica_states ars " +
		"JOIN sys.availability_replicas ar ON ar.replica_id = ars.replica_id " +
		"WHERE ars.is_local = 1 AND ars.database_id = DB_ID();";

	/// <summary>
	/// Build an <c>ALTER AVAILABILITY GROUP … FAILOVER</c> statement targeting the named AG.
	/// Must be executed against the replica that should become the new primary.
	/// </summary>
	/// <param name="availabilityGroupName">
	/// Availability group identifier. Validated to contain only letters, digits, underscore or dash
	/// (max 128 chars) — the AG name cannot be parameterized, so unsafe characters are rejected to
	/// prevent SQL injection.
	/// </param>
	/// <param name="allowDataLoss">
	/// When true, emits <c>FORCE_FAILOVER_ALLOW_DATA_LOSS</c>. Use only when the previous primary
	/// is unrecoverable and the operator accepts data loss.
	/// </param>
	public static string BuildForceFailoverSql(string availabilityGroupName, bool allowDataLoss = false)
	{
		ValidateAvailabilityGroupName(availabilityGroupName);
		return allowDataLoss
			? $"ALTER AVAILABILITY GROUP [{availabilityGroupName}] FORCE_FAILOVER_ALLOW_DATA_LOSS;"
			: $"ALTER AVAILABILITY GROUP [{availabilityGroupName}] FAILOVER;";
	}

	private static void ValidateAvailabilityGroupName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException("Availability group name must not be empty.", nameof(name));
		}

		if (!AvailabilityGroupNameRegex().IsMatch(name))
		{
			throw new ArgumentException(
				"Availability group name may contain only letters, digits, underscore or dash (max 128 chars).",
				nameof(name));
		}
	}

	[GeneratedRegex("^[A-Za-z0-9_\\-]{1,128}$")]
	private static partial Regex AvailabilityGroupNameRegex();
}
