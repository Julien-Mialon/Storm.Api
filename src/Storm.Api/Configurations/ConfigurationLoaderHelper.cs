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
			if(delimiterIndex >= 0)
			{
				environmentTypePart = environmentTypePart.Substring(delimiterIndex + 1);
			}
			switch (environmentTypePart)
			{
				case "dev":
					EnvironmentHelper.Set(EnvironmentSlot.Dev);
					break;
				case "test":
					EnvironmentHelper.Set(EnvironmentSlot.Test);
					break;
				case "alpha":
					EnvironmentHelper.Set(EnvironmentSlot.Alpha);
					break;
				case "beta":
					EnvironmentHelper.Set(EnvironmentSlot.Beta);
					break;
				case "prod":
					EnvironmentHelper.Set(EnvironmentSlot.Prod);
					break;
				case "docker":
					EnvironmentHelper.Set(EnvironmentSlot.Container);
					break;
				default:
					name = "local";
					EnvironmentHelper.Set(EnvironmentSlot.Local);
					break;
			}

			configurationBuilder
				.AddJsonFile("appsettings.json", true, true)
				.AddJsonFile($"appsettings.{name}.json", true, true)
				.AddEnvironmentVariables();
		}
	}
}