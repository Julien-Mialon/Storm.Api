using Microsoft.Extensions.Logging;
using Storm.Api.Extensions;
using Storm.Api.Logs.Interfaces;
using Storm.Api.Logs.Internals;
using Storm.Api.Logs.Sinks.Consoles;

namespace Storm.Api.Logs;

public class LogService : ILogService
{
	private Lazy<ILogSink> _sink;
	private readonly LogLevel _minimumLogLevel;
	private readonly List<ILogAppender> _appenders = new();

	public LogService(Func<ILogService, ILogSink> senderFactory, LogLevel minimumLogLevel)
	{
		_sink = new(() => senderFactory(this));
		_minimumLogLevel = minimumLogLevel;
	}

	protected LogService(LogLevel minimumLogLevel)
	{
		_minimumLogLevel = minimumLogLevel;
		_sink = new(() => new ConsoleLogSink());
	}

	protected void UseSink(ILogSink sink)
	{
		_sink = new(sink);
	}

	public ILogService WithAppender(ILogAppender appender)
	{
		if (appender.MultipleAllowed is false && _appenders.Any(x => x.GetType() == appender.GetType()))
		{
			// ignoring multiple add calls, this could lead to invalid json data
			return this;
		}

		_appenders.Add(appender);
		return this;
	}

	public ILogService WithAppenders(params ILogAppender[] appenders)
	{
		foreach (ILogAppender appender in appenders)
		{
			WithAppender(appender);
		}

		return this;
	}

	public void Log(LogLevel level, Action<IObjectWriter> fillLogEntry)
	{
		if (_minimumLogLevel > level)
		{
			return;
		}

		using JsonLogWriter content = new();
		fillLogEntry(content);
		((IObjectWriter)content).WriteProperty("log_level", level.ToString());
		for (int i = 0 ; i < _appenders.Count ; i++)
		{
			_appenders[i].Append(content);
		}

		string entry = content.ToString();
		if (entry.IsNullOrEmpty())
		{
			return;
		}

		_sink.Value.Enqueue(level, entry);
	}
}