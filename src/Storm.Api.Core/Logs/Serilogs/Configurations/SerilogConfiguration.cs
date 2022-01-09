using Serilog;
using Storm.Api.Core.Logs.Serilogs.Senders;

namespace Storm.Api.Core.Logs.Serilogs.Configurations;

public class SerilogConfiguration
{
	internal string LogFileName { get; set; } = "output.log";

	internal bool EnableConsoleLogging { get; set; } = false;
	internal LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

	public ILogService CreateService()
	{
		return new LogService(CreateSender, MinimumLogLevel);
	}

	private ILogSender CreateSender(ILogService logService)
	{
		LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
			.MinimumLevel.Is(MinimumLogLevel.AsLogEventLevel())
			.WriteTo.File($"logs/{LogFileName}", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true);

		if (EnableConsoleLogging)
		{
			loggerConfiguration = loggerConfiguration.WriteTo.Console();
		}

		return new SerilogSender(loggerConfiguration.CreateLogger());
	}

	public static ISerilogConfigurationBuilder CreateBuilder() => new SerilogConfigurationBuilder();
}