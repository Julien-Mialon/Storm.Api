using System;
using System.Data;
using System.Threading.Tasks;
using ServiceStack.OrmLite;

namespace Storm.Api.Core.Databases
{
	public interface IDatabaseService : IDisposable
	{
		Task<IDbConnection> Connection { get; }

		Task<IDatabaseTransaction> Transaction();
	}

	internal sealed class DatabaseService : IDatabaseService
	{
		private readonly IDatabaseConnectionFactory _factory;
		private bool _connectionOpened;
		private bool _disposed;
		private Task<IDbConnection> _connection;
		private IDatabaseTransaction _transaction;

		public DatabaseService(IDatabaseConnectionFactory factory)
		{
			_factory = factory;
		}

		public Task<IDbConnection> Connection => GetConnection();

		public async Task<IDatabaseTransaction> Transaction()
		{
			if (_transaction != null)
			{
				return _transaction;
			}

			IDbConnection connection = await Connection;
			IDbTransaction transaction = connection.OpenTransaction();

			return _transaction = new DatabaseTransaction(transaction, this);
		}

		private Task<IDbConnection> GetConnection()
		{
			if (_connectionOpened)
			{
				return _connection;
			}

			_connection = _factory.Open();
			_connectionOpened = true;
			return _connection;
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			if (_connectionOpened && _connection.IsCompleted && !_connection.IsFaulted && !_connection.IsCanceled)
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
}