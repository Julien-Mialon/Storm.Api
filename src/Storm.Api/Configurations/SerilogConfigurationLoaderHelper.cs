using System;
using Microsoft.Extensions.Configuration;
using Storm.Api.Core.Logs;
using Storm.Api.Core.Logs.Serilogs.Configurations;

namespace Storm.Api.Configurations
{
	public static class SerilogConfigurationLoaderHelper
	{
		public static ISerilogConfigurationBuilder FromConfiguration(this ISerilogConfigurationBuilder builder, IConfiguration configuration)
		{
			/* Keys : LogLevel, EnableConsole, */

			if (configuration.GetValue<string>("LogLevel", null) is { } minimumLogLevelString && Enum.TryParse<LogLevel>(minimumLogLevelString, out LogLevel minimumLogLevel))
			{
				builder = builder.WithMinimumLogLevel(minimumLogLevel);
			}

			if (configuration.GetValue<bool>("EnableConsole", false))
			{
				builder = builder.WithConsoleOutput();
			}

			return builder;
		}
	}
}