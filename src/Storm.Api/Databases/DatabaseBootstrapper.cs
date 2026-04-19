using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;
using Storm.Api.Databases.Configurations;
using Storm.Api.Databases.Configurations.HighAvailability;
using Storm.Api.Databases.Connections;
using Storm.Api.Databases.Connections.HighAvailability;
using Storm.Api.Databases.Middlewares;
using Storm.Api.Databases.Services;
using Storm.Api.Databases.Workers;

namespace Storm.Api.Databases;

public static class DatabaseBootstrapper
{
	public static IServiceCollection AddDatabaseModule(this IServiceCollection services, DatabaseConfigurationBuilder databaseConfigurationBuilder, bool skipLicenceCheck = false)
	{
		if (LicenseUtils.HasLicensedFeature(LicenseFeature.OrmLite) is false && skipLicenceCheck is false)
		{
			throw new InvalidOperationException("You need a license to use OrmLite");
		}

		IDatabaseConnectionFactory factory = databaseConfigurationBuilder.Build();
		services
			.AddSingleton(factory)
			.AddScoped<IDatabaseService, DatabaseService>()
			.AddSingleton<IDatabaseServiceAccessor, DatabaseServiceAccessor>();

		if (databaseConfigurationBuilder.BuiltManager is { } manager
			&& databaseConfigurationBuilder.HighAvailabilityOptionsOrNull is { } haOptions)
		{
			services.AddSingleton<IDatabaseReplicaManager>(manager);
			services.AddSingleton(haOptions);
			services.AddHostedService<DatabaseReplicaHealthWorker>();

			try
			{
				using CancellationTokenSource cts = new(TimeSpan.FromSeconds(10));
				manager.ProbeAllAsync(cts.Token).GetAwaiter().GetResult();
			}
			catch
			{
				// Swallow startup probe failures: the health worker will retry after the app starts.
			}
		}

		return services;
	}

	public static IApplicationBuilder UseDatabaseModule(this IApplicationBuilder app)
	{
		return app.UseDisposeConnectionMiddleware();
	}
}
