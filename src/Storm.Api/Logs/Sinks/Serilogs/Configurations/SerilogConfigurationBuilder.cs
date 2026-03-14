using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Storm.Api.Logs.Sinks.Serilogs.Configurations;

internal class SerilogConfigurationBuilder : ISerilogConfigurationBuilder
{
	[AllowNull]
	private SerilogConfiguration Configuration { get => field ?? throw new InvalidOperationException("You can not change parameters in builder after calling Build()"); set; } = new();

	public SerilogConfiguration Build()
	{
		SerilogConfiguration configuration = Configuration;
		Configuration = null;
		return configuration;
	}

	public ISerilogConfigurationBuilder WithOutputFile(string file)
	{
		Configuration.LogFileName = file;
		return this;
	}

	public ISerilogConfigurationBuilder WithConsoleOutput()
	{
		Configuration.EnableConsoleLogging = true;
		return this;
	}

	public ISerilogConfigurationBuilder WithMinimumLogLevel(LogLevel minimumLogLevel)
	{
		Configuration.MinimumLogLevel = minimumLogLevel;
		return this;
	}
}