using System;
using System.Data;

namespace Storm.Api.Core.Databases
{
	public interface IDatabaseTransaction : IDisposable
	{
		void Commit();

		void Rollback();
	}

	internal class DatabaseTransaction : IDatabaseTransaction
	{
		private DatabaseService _databaseService;
		private IDbTransaction _transaction;
		private bool _finalized;

		public DatabaseTransaction(IDbTransaction transaction, DatabaseService databaseService)
		{
			_transaction = transaction;
			_databaseService = databaseService;
		}

		public void Dispose()
		{
			if (!_finalized)
			{
				_finalized = true;
				_transaction.Commit();

				_databaseService.EndTransaction(this);
			}
			_transaction.Dispose();
			_transaction = null;
			_databaseService = null;
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
}