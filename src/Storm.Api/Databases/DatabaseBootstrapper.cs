using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;
using Storm.Api.Databases.Configurations;
using Storm.Api.Databases.Middlewares;
using Storm.Api.Databases.Services;
using Storm.Api.Launchers;

namespace Storm.Api.Databases;

public static class DatabaseBootstrapper
{
	public static IServiceCollection AddDatabaseModule(this IServiceCollection services, DatabaseConfigurationBuilder databaseConfigurationBuilder, bool skipLicenceCheck = false)
	{
		if (DefaultLauncherOptions.SkipOrmLiteLicenseCheck is false && skipLicenceCheck is false && LicenseUtils.HasLicensedFeature(LicenseFeature.OrmLite) is false)
		{
			throw new InvalidOperationException("You need a license to use OrmLite");
		}

		return services
			.AddSingleton(databaseConfigurationBuilder.Build())
			.AddScoped<IDatabaseService, DatabaseService>()
			.AddSingleton<IDatabaseServiceAccessor, DatabaseServiceAccessor>();
	}

	public static IApplicationBuilder UseDatabaseModule(this IApplicationBuilder app)
	{
		return app.UseDisposeConnectionMiddleware();
	}
}