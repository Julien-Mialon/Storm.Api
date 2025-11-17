using Microsoft.Extensions.Logging;

namespace Storm.Api.Logs.Sinks.Serilogs.Configurations;

internal class SerilogConfigurationBuilder : ISerilogConfigurationBuilder
{
	private SerilogConfiguration? _configuration = new();
	private SerilogConfiguration Configuration => _configuration ?? throw new InvalidOperationException("You can not change parameters in builder after calling Build()");

	public SerilogConfiguration Build()
	{
		SerilogConfiguration configuration = Configuration;
		_configuration = null;
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