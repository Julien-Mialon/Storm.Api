using System.Data;
using Storm.Api.Services;

namespace Storm.Api.Databases.Services;

internal class DatabaseServiceAccessor : IDatabaseServiceAccessor
{
	private readonly IScopedServiceAccessor _scopeServiceAccessor;

	public IDatabaseService DatabaseService => _scopeServiceAccessor.Get<IDatabaseService>();

	public Task<IDbConnection> Connection => DatabaseService.Connection;

	public DatabaseServiceAccessor(IScopedServiceAccessor scopeServiceAccessor)
	{
		_scopeServiceAccessor = scopeServiceAccessor;
	}
}