using Microsoft.Extensions.Logging;
using Serilog.Core;
using Storm.Api.Logs.Interfaces;
using Storm.Api.Logs.Sinks.Serilogs.Extensions;

namespace Storm.Api.Logs.Sinks.Serilogs;

internal class SerilogSink : ILogSink
{
	private readonly Logger _logger;

	public SerilogSink(Logger logger)
	{
		_logger = logger;
	}

	public void Enqueue(LogLevel level, string entry)
	{
		_logger.Write(level.AsLogEventLevel(), entry);
	}
}