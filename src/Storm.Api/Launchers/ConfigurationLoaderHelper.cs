using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Storm.Api.Launchers;

public static class ConfigurationLoaderHelper
{
	public static IConfigurationBuilder LoadConfiguration(this IConfigurationBuilder configurationBuilder, IHostEnvironment hostingEnvironment)
	{
		EnvironmentHelper.SetFromEnvironment(hostingEnvironment.EnvironmentName);

		configurationBuilder
			.AddJsonFile("appsettings.json", true, reloadOnChange: false)
			.AddJsonFile("projectsettings.json", true, reloadOnChange: false)
			.AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", true, reloadOnChange: false)
			.AddJsonFile($"projectsettings.{hostingEnvironment.EnvironmentName}.json", true, reloadOnChange: false)
			.AddEnvironmentVariables();

		return configurationBuilder;
	}
}