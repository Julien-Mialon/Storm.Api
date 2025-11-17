using Microsoft.Extensions.Logging;

namespace Storm.Api.Logs.Sinks.Serilogs.Configurations;

public interface ISerilogConfigurationBuilder
{
	SerilogConfiguration Build();
	ISerilogConfigurationBuilder WithOutputFile(string file);
	ISerilogConfigurationBuilder WithConsoleOutput();
	ISerilogConfigurationBuilder WithMinimumLogLevel(LogLevel minimumLogLevel);
}