using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Configurations;
using Storm.Api.Core.Databases;

namespace Storm.SqlMigrations;

public static class MigrationHelper
{
	public static async Task<int> Run(string[] args, params IMigrationModule[] modules)
	{
		if (args.Length == 0)
		{
			string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
			if (string.IsNullOrEmpty(environment))
			{
				Console.WriteLine("Usage: SqlMigrations <environment>");
				return -42;
			}

			args = new[]
			{
				environment
			};
		}

		IConfiguration configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json")
			.AddJsonFile($"appsettings.{args[0]}.json", optional: false, reloadOnChange: false)
			.Build();

		IServiceCollection services = new ServiceCollection();
		services.AddDatabaseModule(configuration.GetSection("Database").LoadDatabaseConfiguration());
		IServiceProvider providers = services.BuildServiceProvider();

		using (providers.CreateScope())
		{
			MigrationEngine migrations = new MigrationEngine(providers, modules);
			return await migrations.Run() ? 0 : -1;
		}
	}
}