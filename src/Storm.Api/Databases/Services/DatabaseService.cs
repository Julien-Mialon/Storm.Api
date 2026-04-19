using System.Data;
using ServiceStack.OrmLite;
using Storm.Api.Databases.Connections;

namespace Storm.Api.Databases.Services;

public class DatabaseService : IDatabaseService
{
	private readonly IDatabaseConnectionFactory _factory;
	private bool _writeOpened;
	private bool _readOpened;
	private bool _disposed;
	private Task<IDbConnection>? _writeConnection;
	private Task<IDbConnection>? _readConnection;
	private IDatabaseTransaction? _transaction;

	public Task<IDbConnection> Connection => GetWriteConnection();

	public DatabaseService(IDatabaseConnectionFactory factory)
	{
		_factory = factory;
	}

	public Task<IDbConnection> GetConnection(CancellationToken ct = default)
		=> GetWriteConnection(ct);

	public Task<IDbConnection> GetWriteConnection(CancellationToken ct = default)
	{
		if (_writeOpened)
		{
			return _writeConnection!;
		}

		_writeConnection = _factory.OpenWrite(ct);
		_writeOpened = true;
		return _writeConnection;
	}

	public Task<IDbConnection> GetReadConnection(CancellationToken ct = default)
	{
		if (!_factory.SupportsReadReplicas)
		{
			return GetWriteConnection(ct);
		}

		if (_readOpened)
		{
			return _readConnection!;
		}

		_readConnection = _factory.OpenRead(ct);
		_readOpened = true;
		return _readConnection;
	}

	public async Task<IDatabaseTransaction> CreateTransaction(IsolationLevel? isolationLevel = null, CancellationToken ct = default)
	{
		IDbConnection connection = await GetWriteConnection(ct);
		if (_transaction is not null)
		{
			return new DummyTransaction(connection);
		}

		IDbTransaction transaction = isolationLevel.HasValue ? connection.OpenTransaction(isolationLevel.Value) : connection.OpenTransaction();

		return _transaction = new DatabaseTransaction(connection, transaction, this);
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

		DisposeCachedConnection(_writeOpened, _writeConnection);
		DisposeCachedConnection(_readOpened, _readConnection);

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

	private static void DisposeCachedConnection(bool opened, Task<IDbConnection>? connectionTask)
	{
		if (opened && connectionTask is { IsCompleted: true, IsFaulted: false, IsCanceled: false })
		{
			connectionTask.Result.Dispose();
		}
	}
}
