using System.Data;
using Storm.Api.Databases.Configurations.HighAvailability;

namespace Storm.Api.Databases.Connections.HighAvailability;

/// <summary>
/// Orchestrates the SQL Server HA replica pool: probes replicas for role/health, routes
/// read and write connections, and exposes state to the framework user.
/// </summary>
public interface IDatabaseReplicaManager
{
	/// <summary>True when the latest probe found a reachable primary.</summary>
	bool IsPrimaryAvailable { get; }

	/// <summary>
	/// Throws <see cref="CQRS.Exceptions.DomainDatabaseException"/> when no primary is available.
	/// Intended as the "write gate" to guard critical write paths.
	/// </summary>
	void EnsurePrimaryAvailable();

	/// <summary>Snapshot of every replica's latest observed status.</summary>
	IReadOnlyList<ReplicaStatus> GetReplicaStates();

	/// <summary>Open a connection against the current primary. Throws if unavailable.</summary>
	Task<IDbConnection> OpenWriteAsync(CancellationToken ct = default);

	/// <summary>
	/// Open a connection against a read-eligible secondary. Falls back to primary per options;
	/// throws when fallback is disabled and no eligible replica is available.
	/// </summary>
	Task<IDbConnection> OpenReadAsync(CancellationToken ct = default);

	/// <summary>Run a round of health probes against every configured replica.</summary>
	Task ProbeAllAsync(CancellationToken ct = default);

	/// <summary>
	/// Execute an <c>ALTER AVAILABILITY GROUP … FAILOVER</c> statement against the named host,
	/// then immediately probe the pool so state reflects the new topology.
	/// </summary>
	Task PromoteToPrimaryAsync(string host, string availabilityGroupName, bool allowDataLoss = false, CancellationToken ct = default);
}
