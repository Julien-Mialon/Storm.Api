using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Databases.Connections;

namespace Storm.Api.Databases.Services;

public abstract class BaseDatabaseService : BaseServiceContainer
{
	protected IDatabaseService DatabaseService => Services.GetRequiredService<IDatabaseService>();

	protected BaseDatabaseService(IServiceProvider services) : base(services)
	{
	}

	protected async Task<T> UseConnection<T>(Func<IDbConnection, Task<T>> executor)
	{
		IDbConnection connection = await DatabaseService.GetWriteConnection();
		return await executor(connection);
	}

	protected async Task UseConnection(Func<IDbConnection, Task> executor)
	{
		IDbConnection connection = await DatabaseService.GetWriteConnection();
		await executor(connection);
	}

	protected async Task<T> UseReadConnection<T>(Func<IDbConnection, Task<T>> executor)
	{
		IDbConnection connection = await DatabaseService.GetReadConnection();
		return await executor(connection);
	}

	protected async Task UseReadConnection(Func<IDbConnection, Task> executor)
	{
		IDbConnection connection = await DatabaseService.GetReadConnection();
		await executor(connection);
	}

	protected async Task<T> UseWriteConnection<T>(Func<IDbConnection, Task<T>> executor)
	{
		IDbConnection connection = await DatabaseService.GetWriteConnection();
		return await executor(connection);
	}

	protected async Task UseWriteConnection(Func<IDbConnection, Task> executor)
	{
		IDbConnection connection = await DatabaseService.GetWriteConnection();
		await executor(connection);
	}

	/// <summary>
	/// Write gate. Throws <see cref="CQRS.Exceptions.DomainDatabaseException"/> when no primary
	/// replica is available. No-op in single-node setups.
	/// </summary>
	protected void EnsurePrimaryAvailable()
	{
		Resolve<IDatabaseConnectionFactory>().EnsurePrimaryAvailable();
	}

	protected async Task<T> WithDatabaseTransaction<T>(Func<IDbConnection, IDatabaseTransaction, Task<T>> executor, IsolationLevel? isolationLevel = null)
	{
		IDatabaseService databaseService = Resolve<IDatabaseService>();
		return await databaseService.InTransaction<T>(transaction => executor(transaction.Connection, transaction), isolationLevel);
	}

	protected async Task WithDatabaseTransaction(Func<IDbConnection, IDatabaseTransaction, Task> executor, IsolationLevel? isolationLevel = null)
	{
		IDatabaseService databaseService = Resolve<IDatabaseService>();
		await databaseService.InTransaction(transaction => executor(transaction.Connection, transaction), isolationLevel);
	}

	protected async Task<T> WithDatabaseTransaction<T>(Func<Task<T>> executor, IsolationLevel? isolationLevel = null)
	{
		IDatabaseService databaseService = Resolve<IDatabaseService>();
		return await databaseService.InTransaction<T>(_ => executor(), isolationLevel);
	}

	protected async Task WithDatabaseTransaction(Func<Task> executor, IsolationLevel? isolationLevel = null)
	{
		IDatabaseService databaseService = Resolve<IDatabaseService>();
		await databaseService.InTransaction(_ => executor(), isolationLevel);
	}
}
