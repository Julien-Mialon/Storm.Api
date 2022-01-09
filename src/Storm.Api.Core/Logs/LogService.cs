using Storm.Api.Core.Logs.Consoles;
using Storm.Api.Core.Logs.Internals;

namespace Storm.Api.Core.Logs;

public interface ILogService
{
	void Log(LogLevel level, Action<IObjectWriter> fillLogEntry);

	ILogService WithAppender(ILogAppender appender);
}

public class LogService : ILogService
{
	private Lazy<ILogSender> _sender;
	private readonly LogLevel _minimumLogLevel;
	private readonly List<ILogAppender> _appenders = new();

	public LogService(Func<ILogService, ILogSender> senderFactory, LogLevel minimumLogLevel)
	{
		_sender = new(() => senderFactory(this));
		_minimumLogLevel = minimumLogLevel;
	}

	protected LogService(LogLevel minimumLogLevel)
	{
		_minimumLogLevel = minimumLogLevel;
		_sender = new(() => new ConsoleLogSender());
	}

	protected void UseSender(ILogSender sender)
	{
		_sender = new(() => sender);
	}

	public ILogService WithAppender(ILogAppender appender)
	{
		_appenders.Add(appender);
		return this;
	}

	public void Log(LogLevel level, Action<IObjectWriter> fillLogEntry)
	{
		if (_minimumLogLevel > level)
		{
			return;
		}

		IObjectWriter content = new JsonLogWriter();
		fillLogEntry(content);
		content.WriteProperty("log_level", level.ToString());
		for (int i = 0 ; i < _appenders.Count ; i++)
		{
			_appenders[i].Append(content);
		}
		_sender.Value.Enqueue(level, content.ToString());
	}
}