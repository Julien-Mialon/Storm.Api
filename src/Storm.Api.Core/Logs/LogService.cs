using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Core.Logs.Internals;

namespace Storm.Api.Core.Logs
{
	public interface ILogService
	{
		void Log(LogLevel level, Action<IObjectWriter> fillLogEntry);

		ILogService WithAppender(ILogAppender appender);
	}

	public class LogService : ILogService
	{
		private readonly Lazy<ILogSender> _sender;
		private readonly LogLevel _minimumLogLevel;
		private readonly List<ILogAppender> _appenders = new List<ILogAppender>();

		public LogService(Func<ILogService, ILogSender> senderFactory, LogLevel minimumLogLevel)
		{
			_sender = new Lazy<ILogSender>(() => senderFactory(this));
			_minimumLogLevel = minimumLogLevel;
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
}
