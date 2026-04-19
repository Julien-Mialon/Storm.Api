using System.Data;

namespace Storm.Api.Databases.Connections;

/// <summary>
/// Connection factory to use database
/// </summary>
public interface IDatabaseConnectionFactory
{
	/// <summary>
	/// Create a database connection but do not open it yet
	/// </summary>
	/// <returns>The newly created connection</returns>
	IDbConnection Create();

	/// <summary>
	/// Create a database connection and open it.
	/// For HA-aware factories this is equivalent to <see cref="OpenWrite" /> (primary).
	/// </summary>
	Task<IDbConnection> Open(CancellationToken ct) => OpenWrite(ct);

	/// <summary>
	/// Open a connection intended for write operations. Always targets the primary when HA is enabled.
	/// Throws <see cref="CQRS.Exceptions.DomainDatabaseException"/> if no primary is available.
	/// </summary>
	Task<IDbConnection> OpenWrite(CancellationToken ct);

	/// <summary>
	/// Open a connection intended for read-only operations. Targets a healthy secondary when HA
	/// is enabled, falling back to the primary based on configuration.
	/// </summary>
	Task<IDbConnection> OpenRead(CancellationToken ct);

	/// <summary>
	/// Synchronously throws <see cref="CQRS.Exceptions.DomainDatabaseException"/> when the primary
	/// is currently unavailable. No-op in single-node setups.
	/// </summary>
	void EnsurePrimaryAvailable()
	{
	}

	/// <summary>
	/// True when this factory distinguishes read replicas from the primary. When false,
	/// <c>DatabaseService</c> reuses the write connection for read operations (back-compat).
	/// </summary>
	bool SupportsReadReplicas => false;
}
