using Microsoft.Extensions.Logging;
using Serilog;
using Storm.Api.Logs.Interfaces;
using Storm.Api.Logs.Sinks.Serilogs.Extensions;

namespace Storm.Api.Logs.Sinks.Serilogs.Configurations;

public class SerilogConfiguration
{
	internal string? LogFileName { get; set; } = null;

	internal bool EnableConsoleLogging { get; set; } = false;
	internal LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

	public ILogService CreateService()
	{
		return new LogService(CreateSender, MinimumLogLevel);
	}

	private ILogSink CreateSender(ILogService logService)
	{
		LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
			.MinimumLevel.Is(MinimumLogLevel.AsLogEventLevel());

		if (LogFileName is not null)
		{
			loggerConfiguration = loggerConfiguration.WriteTo.File(LogFileName, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true);
		}

		if (EnableConsoleLogging)
		{
			loggerConfiguration = loggerConfiguration.WriteTo.Console();
		}

		return new SerilogSink(loggerConfiguration.CreateLogger());
	}

	public static ISerilogConfigurationBuilder CreateBuilder() => new SerilogConfigurationBuilder();
}