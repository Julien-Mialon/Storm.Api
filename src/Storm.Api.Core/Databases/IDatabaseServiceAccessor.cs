using System.Data;

namespace Storm.Api.Core.Databases;

public interface IDatabaseServiceAccessor
{
	IDatabaseService DatabaseService { get; }

	Task<IDbConnection> Connection { get; }
}