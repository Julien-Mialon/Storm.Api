using Microsoft.Extensions.DependencyInjection;

namespace Storm.Api.Core.Databases
{
	public static class DatabaseBootstrapper
	{
		public static IServiceCollection AddDatabaseModule(this IServiceCollection services, DatabaseConfigurationBuilder databaseConfigurationBuilder)
		{
			return services
				.AddSingleton<IDatabaseConnectionFactory>(databaseConfigurationBuilder.Build())
				.AddScoped<IDatabaseService, DatabaseService>()
				.AddSingleton<IDatabaseServiceAccessor, DatabaseServiceAccessor>();
		}
	}
}