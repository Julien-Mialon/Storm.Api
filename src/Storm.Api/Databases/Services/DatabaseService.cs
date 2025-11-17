using System.Data;
using ServiceStack.OrmLite;
using Storm.Api.Databases.Connections;

namespace Storm.Api.Databases.Services;

public class DatabaseService : IDatabaseService
{
	private readonly IDatabaseConnectionFactory _factory;
	private bool _connectionOpened;
	private bool _disposed;
	private Task<IDbConnection>? _connection;
	private IDatabaseTransaction? _transaction;

	public Task<IDbConnection> Connection => GetConnection();

	public DatabaseService(IDatabaseConnectionFactory factory)
	{
		_factory = factory;
	}

	public Task<IDbConnection> GetConnection(CancellationToken ct = default)
	{
		if (_connectionOpened)
		{
			return _connection!;
		}

		_connection = _factory.Open(ct);
		_connectionOpened = true;
		return _connection;
	}

	public async Task<IDatabaseTransaction> CreateTransaction(IsolationLevel? isolationLevel = null, CancellationToken ct = default)
	{
		IDbConnection connection = await Connection;
		if (_transaction is not null)
		{
			return new DummyTransaction(connection);
		}

		IDbTransaction transaction = isolationLevel.HasValue ? connection.OpenTransaction(isolationLevel.Value) : connection.OpenTransaction();

		return _transaction = new DatabaseTransaction(transaction, this);
	}

	public async Task InTransaction(Func<IDatabaseTransaction, Task> action, IsolationLevel? isolationLevel = null, CancellationToken ct = default)
	{
		using IDatabaseTransaction transaction = await CreateTransaction(isolationLevel, ct);
		bool isTransactionClosed = false;
		try
		{
			await action(transaction);
		}
		catch
		{
			transaction.Rollback();
			isTransactionClosed = true;
			throw;
		}
		finally
		{
			if (isTransactionClosed is false)
			{
				transaction.Commit();
			}
		}
	}

	public async Task<T> InTransaction<T>(Func<IDatabaseTransaction, Task<T>> action, IsolationLevel? isolationLevel = null, CancellationToken ct = default)
	{
		using IDatabaseTransaction transaction = await CreateTransaction(isolationLevel, ct);
		bool isTransactionClosed = false;
		try
		{
			return await action(transaction);
		}
		catch
		{
			transaction.Rollback();
			isTransactionClosed = true;
			throw;
		}
		finally
		{
			if (isTransactionClosed is false)
			{
				transaction.Commit();
			}
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		if (_connectionOpened && _connection!.IsCompleted && !_connection.IsFaulted && !_connection.IsCanceled)
		{
			_connection.Result.Dispose();
		}

		if (_transaction != null)
		{
			throw new InvalidOperationException("Leaking database transaction !");
		}

		_disposed = true;
	}

	internal void EndTransaction(DatabaseTransaction transaction)
	{
		if (_transaction == transaction)
		{
			_transaction = null;
		}
	}
}