using Microsoft.Extensions.Configuration;

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

	public static T? WithSection<T>(this IConfiguration configuration, string sectionName, Func<IConfigurationSection, T> action)
	{
		if (configuration.GetSection(sectionName) is { } section && section.Exists())
		{
			return action(section);
		}

		return default;
	}
}