using System.Data;

namespace Storm.Api.Core.Databases;

public interface IDatabaseTransaction : IDisposable
{
	void Commit();

	void Rollback();
}

internal class DatabaseTransaction : IDatabaseTransaction
{
	private readonly DatabaseService _databaseService;
	private readonly IDbTransaction _transaction;
	private bool _finalized;
	private bool _disposed;

	public DatabaseTransaction(IDbTransaction transaction, DatabaseService databaseService)
	{
		_transaction = transaction;
		_databaseService = databaseService;
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;
		if (!_finalized)
		{
			_finalized = true;
			_transaction.Commit();

			_databaseService.EndTransaction(this);
		}
		_transaction.Dispose();
	}

	public void Commit()
	{
		_finalized = true;
		_transaction.Commit();

		_databaseService.EndTransaction(this);
	}

	public void Rollback()
	{
		_finalized = true;
		_transaction.Rollback();

		_databaseService.EndTransaction(this);
	}
}