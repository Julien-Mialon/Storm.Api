using System.Data;

namespace Storm.Api.Databases.Connections;

internal sealed class DummyTransaction : IDatabaseTransaction
{
	public IDbConnection Connection { get; }

	public DummyTransaction(IDbConnection connection)
	{
		Connection = connection;
	}

	public void Dispose() { }
	public void Rollback() { }
	public void Commit() { }
}