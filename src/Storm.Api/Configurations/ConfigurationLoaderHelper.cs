using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Storm.Api.Configurations
{
	public static class ConfigurationLoaderHelper
	{
		public static void LoadConfiguration(this IConfigurationBuilder configurationBuilder, IWebHostEnvironment hostingEnvironment)
		{
			EnvironmentHelper.SetFromEnvironment(hostingEnvironment.EnvironmentName);

			configurationBuilder
				.AddJsonFile("appsettings.json", true, true)
				.AddJsonFile("projectsettings.json", true, true)
				.AddJsonFile($"appsettings.{hostingEnvironment.EnvironmentName}.json", true, true)
				.AddEnvironmentVariables();
		}
	}
}