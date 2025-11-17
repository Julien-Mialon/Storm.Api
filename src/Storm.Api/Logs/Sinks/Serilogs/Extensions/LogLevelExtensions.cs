using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Storm.Api.Logs.Sinks.Serilogs.Extensions;

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