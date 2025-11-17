using System.Data;

namespace Storm.Api.Databases.Services;

public interface IDatabaseServiceAccessor
{
	IDatabaseService DatabaseService { get; }

	Task<IDbConnection> Connection { get; }
}