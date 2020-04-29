using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Storm.Api.Configurations
{
	public static class ConfigurationLoaderHelper
	{
		public static void LoadConfiguration(this IConfigurationBuilder configurationBuilder, IWebHostEnvironment hostingEnvironment)
		{
			string name = hostingEnvironment.EnvironmentName;
			string environmentTypePart = name;

			int delimiterIndex = environmentTypePart.LastIndexOf('-');
			if (delimiterIndex >= 0)
			{
				environmentTypePart = environmentTypePart.Substring(delimiterIndex + 1);
			}

			EnvironmentHelper.Set(environmentTypePart switch
			{
				"dev" => EnvironmentSlot.Dev,
				"test" => EnvironmentSlot.Test,
				"alpha" => EnvironmentSlot.Alpha,
				"beta" => EnvironmentSlot.Beta,
				"prod" => EnvironmentSlot.Prod,
				_ => EnvironmentSlot.Local,
			});

			configurationBuilder
				.AddJsonFile("appsettings.json", true, true)
				.AddJsonFile($"appsettings.{name}.json", true, true)
				.AddEnvironmentVariables();
		}
	}
}