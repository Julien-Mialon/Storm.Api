using System.Data;
using Storm.Api.Core.Services;

namespace Storm.Api.Core.Databases;

internal class DatabaseServiceAccessor : IDatabaseServiceAccessor
{
	private readonly IScopeServiceAccessor _scopeServiceAccessor;

	public IDatabaseService DatabaseService => _scopeServiceAccessor.Get<IDatabaseService>();

	public Task<IDbConnection> Connection => DatabaseService.Connection;

	public DatabaseServiceAccessor(IScopeServiceAccessor scopeServiceAccessor)
	{
		_scopeServiceAccessor = scopeServiceAccessor;
	}
}