using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Databases.Migrations.Models;
using Storm.Api.Databases.Services;
using Storm.Api.Extensions;

namespace Storm.Api.Databases.Migrations;

public static class MigrationHelper
{
	public static async Task<bool> Run(IServiceProvider services, params IMigrationModule[] modules)
	{
		return await services.ExecuteWithScope(async scopedServices =>
		{
			MigrationEngine migrations = new(modules);
			using IDatabaseService databaseService = scopedServices.GetRequiredService<IDatabaseService>();
			return await migrations.Run(databaseService);
		});
	}
}