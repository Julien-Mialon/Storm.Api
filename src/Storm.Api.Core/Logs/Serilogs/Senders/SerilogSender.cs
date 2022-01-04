using Serilog.Core;
using Serilog.Events;

namespace Storm.Api.Core.Logs.Serilogs.Senders;

internal class SerilogSender : ILogSender
{
	private readonly Logger _logger;

	public SerilogSender(Logger logger)
	{
		_logger = logger;
	}

	public void Enqueue(LogLevel level, string entry)
	{
		_logger.Write(level.AsLogEventLevel(), entry);
	}
}

internal static class LogLevelExtensions
{
	public static LogEventLevel AsLogEventLevel(this LogLevel level) => level switch
	{
		LogLevel.Debug => LogEventLevel.Debug,
		LogLevel.Trace => LogEventLevel.Verbose,
		LogLevel.Information => LogEventLevel.Information,
		LogLevel.Warning => LogEventLevel.Warning,
		LogLevel.Error => LogEventLevel.Error,
		LogLevel.Critical => LogEventLevel.Fatal,
		_ => LogEventLevel.Debug
	};
}