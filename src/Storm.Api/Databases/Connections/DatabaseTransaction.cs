using System.Data;
using Storm.Api.Databases.Services;

namespace Storm.Api.Databases.Connections;

internal class DatabaseTransaction : IDatabaseTransaction
{
	private readonly DatabaseService _databaseService;
	private readonly IDbTransaction _transaction;
	private bool _finalized;
	private bool _disposed;

	public IDbConnection Connection => _transaction.Connection!;

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
			EndTransaction(true);
		}

		_transaction.Dispose();
	}

	public void Commit() => EndTransaction(true);

	public void Rollback() => EndTransaction(false);

	private void EndTransaction(bool commit)
	{
		_finalized = true;
		_databaseService.EndTransaction(this);
		try
		{
			if (commit)
			{
				_transaction.Commit();
			}
			else
			{
				_transaction.Rollback();
			}
		}
		catch (InvalidOperationException ex) when (ex.TargetSite?.Name is "ZombieCheck") // This catches errors on transaction already finished because there was an issue at SQL statement level
		{
		}
	}
}