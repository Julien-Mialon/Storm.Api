using System.Data;

namespace Storm.Api.Databases.Connections;

public interface IDatabaseTransaction : IDisposable
{
	IDbConnection Connection { get; }
	
	void Commit();

	void Rollback();
}