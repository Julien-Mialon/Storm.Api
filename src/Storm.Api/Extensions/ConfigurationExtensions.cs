using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Storm.Api.Extensions;

public static class ConfigurationExtensions
{
	public static void OnSection(this IConfiguration configuration, string sectionName, Action<IConfigurationSection> action)
	{
		if (configuration.GetSection(sectionName) is { } section && section.Exists())
		{
			action(section);
		}
	}

	public static string SimpleEnvironmentName(this IHostEnvironment environment)
	{
		string name = environment.EnvironmentName;
		int delimiterIndex = name.LastIndexOf('-');
		if (delimiterIndex > 0)
		{
			name = name.Substring(0, delimiterIndex);
		}

		return name;
	}
}