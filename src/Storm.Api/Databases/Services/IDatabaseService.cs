using System.Data;
using Storm.Api.Databases.Connections;

namespace Storm.Api.Databases.Services;

public interface IDatabaseService : IDisposable
{
	/// <summary>Write connection (primary). Alias kept for back-compat.</summary>
	Task<IDbConnection> Connection { get; }

	/// <summary>Opens and caches the write connection for the current scope.</summary>
	Task<IDbConnection> GetConnection(CancellationToken ct = default);

	/// <summary>Opens and caches a connection targeting the primary (or the single node when HA is off).</summary>
	Task<IDbConnection> GetWriteConnection(CancellationToken ct = default);

	/// <summary>
	/// Opens and caches a connection targeting a read-eligible secondary (or the primary when HA
	/// is off or when fallback-to-primary is allowed and no secondary is eligible).
	/// </summary>
	Task<IDbConnection> GetReadConnection(CancellationToken ct = default);

	Task<IDatabaseTransaction> CreateTransaction(IsolationLevel? isolationLevel = null, CancellationToken ct = default);

	Task InTransaction(Func<IDatabaseTransaction, Task> action, IsolationLevel? isolationLevel = null, CancellationToken ct = default);

	Task<T> InTransaction<T>(Func<IDatabaseTransaction, Task<T>> action, IsolationLevel? isolationLevel = null, CancellationToken ct = default);
}
