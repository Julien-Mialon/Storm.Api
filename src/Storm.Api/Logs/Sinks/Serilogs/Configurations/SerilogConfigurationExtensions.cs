using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Storm.Api.Extensions;

namespace Storm.Api.Logs.Sinks.Serilogs.Configurations;

public static class SerilogConfigurationExtensions
{
	public static SerilogConfiguration LoadSerilogConfiguration(this IConfiguration configuration)
	{
		ISerilogConfigurationBuilder builder = SerilogConfiguration.CreateBuilder();

		configuration.GetValue<string>("File").LetIf(x => x.IsNotNullOrEmpty(), x => builder.WithOutputFile(x));
		configuration.GetValue<bool?>("Console").Let(x => builder.WithConsoleOutput());
		configuration.GetValue<string>("MinimumLogLevel").LetParseEnum<LogLevel>(x => builder.WithMinimumLogLevel(x));

		return builder.Build();
	}
}