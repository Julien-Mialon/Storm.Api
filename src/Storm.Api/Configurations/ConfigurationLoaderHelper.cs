using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Storm.Api.Configurations;

public static class ConfigurationLoaderHelper
{
	public static IConfigurationBuilder LoadConfiguration(this IConfigurationBuilder configurationBuilder, IHostEnvironment hostingEnvironment)
	{
		EnvironmentHelper.SetFromEnvironment(hostingEnvironment.EnvironmentName);

		configurationBuilder
			.AddJsonFile("appsettings.json", true, true)
			.AddJsonFile("projectsettings.json", true, true)
			.AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", true, true)
			.AddJsonFile($"projectsettings.{hostingEnvironment.EnvironmentName}.json", true, true)
			.AddEnvironmentVariables();

		return configurationBuilder;
	}
}