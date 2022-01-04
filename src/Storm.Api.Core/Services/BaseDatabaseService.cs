using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Core.Databases;

namespace Storm.Api.Core.Services;

public abstract class BaseDatabaseService : BaseService
{
	protected IDatabaseService DatabaseService => Services.GetRequiredService<IDatabaseService>();

	protected BaseDatabaseService(IServiceProvider services) : base(services)
	{
	}

	protected async Task<T> UseConnection<T>(Func<IDbConnection, Task<T>> executor)
	{
		IDbConnection connection = await DatabaseService.Connection;
		return await executor(connection);
	}

	protected async Task UseConnection(Func<IDbConnection, Task> executor)
	{
		IDbConnection connection = await DatabaseService.Connection;
		await executor(connection);
	}
}