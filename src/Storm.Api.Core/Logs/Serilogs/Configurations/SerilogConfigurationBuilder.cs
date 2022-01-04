namespace Storm.Api.Core.Logs.Serilogs.Configurations;

public interface ISerilogConfigurationBuilder
{
	SerilogConfiguration Build();
	ISerilogConfigurationBuilder WithOutputFile(string file);
	ISerilogConfigurationBuilder WithConsoleOutput();
	ISerilogConfigurationBuilder WithMinimumLogLevel(LogLevel minimumLogLevel);
}

internal class SerilogConfigurationBuilder : ISerilogConfigurationBuilder
{
	private SerilogConfiguration _configuration = new SerilogConfiguration();
	private SerilogConfiguration Configuration => _configuration ?? throw new InvalidOperationException("You can not change parameters in builder after calling Build()");

	public SerilogConfiguration Build()
	{
		SerilogConfiguration configuration = Configuration;
		_configuration = new SerilogConfiguration();
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