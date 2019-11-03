using System;
using Microsoft.Extensions.Configuration;

namespace Storm.Api.Extensions
{
	public static class ConfigurationExtensions
	{
		public static void OnSection(this IConfiguration configuration, string sectionName, Action<IConfigurationSection> action)
		{
			if (configuration.GetSection(sectionName) is { } section)
			{
				action(section);
			}
		}
	}
}