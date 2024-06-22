using Microsoft.Extensions.DependencyInjection;
using ServiceStack;

namespace Storm.Api.Core.Databases;

public static class DatabaseBootstrapper
{
	public static IServiceCollection AddDatabaseModule(this IServiceCollection services, DatabaseConfigurationBuilder databaseConfigurationBuilder)
	{
		if (LicenseUtils.HasLicensedFeature(LicenseFeature.OrmLite) is false)
		{
			throw new InvalidOperationException("You need a license to use OrmLite");
		}

		return services
			.AddSingleton(databaseConfigurationBuilder.Build())
			.AddScoped<IDatabaseService, DatabaseService>()
			.AddSingleton<IDatabaseServiceAccessor, DatabaseServiceAccessor>();
	}
}