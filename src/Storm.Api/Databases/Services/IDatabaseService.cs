using System.Data;
using Storm.Api.Databases.Connections;

namespace Storm.Api.Databases.Services;

public interface IDatabaseService : IDisposable
{
	Task<IDbConnection> Connection { get; }

	Task<IDbConnection> GetConnection(CancellationToken ct = default);

	Task<IDatabaseTransaction> CreateTransaction(IsolationLevel? isolationLevel = null, CancellationToken ct = default);

	Task InTransaction(Func<IDatabaseTransaction, Task> action, IsolationLevel? isolationLevel = null, CancellationToken ct = default);

	Task<T> InTransaction<T>(Func<IDatabaseTransaction, Task<T>> action, IsolationLevel? isolationLevel = null, CancellationToken ct = default);
}