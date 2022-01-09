using ServiceStack.Logging;
using Storm.Api.Core.Extensions;
using Storm.Api.Core.Logs;

namespace Storm.Api.Core.Databases;

public class LogServiceLogFactory : ILogFactory
{
	private readonly bool _debugEnabled;

	public LogServiceLogFactory(bool debugEnabled)
	{
		_debugEnabled = debugEnabled;
	}

	public ILog GetLogger(Type type)
	{
		return new LogServiceDatabaseLog(_debugEnabled, type.Name);
	}

	public ILog GetLogger(string typeName)
	{
		return new LogServiceDatabaseLog(_debugEnabled, typeName);
	}
}

public class LogServiceDatabaseLog : ILog
{
	public static ILogService? LogService { get; set; }

	private readonly string _type;
	public bool IsDebugEnabled { get; }

	public LogServiceDatabaseLog(bool isDebugEnabled, string type)
	{
		_type = type;
		IsDebugEnabled = isDebugEnabled;
	}

	public void Debug(object message) => Log(LogLevel.Debug, message);

	public void Debug(object message, Exception exception) => Log(LogLevel.Debug, message, exception);

	public void DebugFormat(string format, params object[] args) => Log(LogLevel.Debug, format, args);

	public void Error(object message) => Log(LogLevel.Error, message);

	public void Error(object message, Exception exception) => Log(LogLevel.Error, message, exception);

	public void ErrorFormat(string format, params object[] args) => Log(LogLevel.Error, format, args);

	public void Fatal(object message) => Log(LogLevel.Critical, message);

	public void Fatal(object message, Exception exception) => Log(LogLevel.Critical, message, exception);

	public void FatalFormat(string format, params object[] args) => Log(LogLevel.Critical, format, args);

	public void Info(object message) => Log(LogLevel.Information, message);

	public void Info(object message, Exception exception) => Log(LogLevel.Information, message, exception);

	public void InfoFormat(string format, params object[] args) => Log(LogLevel.Information, format, args);

	public void Warn(object message) => Log(LogLevel.Warning, message);

	public void Warn(object message, Exception exception) => Log(LogLevel.Warning, message, exception);

	public void WarnFormat(string format, params object[] args) => Log(LogLevel.Warning, format, args);

	private void Log(LogLevel level, object message, Exception? ex = null)
	{
		if (ex is null)
		{
			LogService?.Log(level, x => x
				.WriteProperty("loggerType", _type)
				.WriteProperty("message", message.ToString())
			);
		}
		else
		{
			LogService?.Log(level, x => x
				.WriteProperty("loggerType", _type)
				.WriteProperty("message", message.ToString())
				.WriteException(ex)
			);
		}
	}

	private void Log(LogLevel level, string format, object[] args)
	{
		LogService?.Log(level, x => x
			.WriteProperty("loggerType", _type)
			.WriteProperty("message", string.Format(format, args))
			.WriteProperty("format", format)
			.WriteArray("args", y =>
			{
				foreach (object arg in args)
				{
					y.WriteValue(arg.ToString());
				}
			})
		);
	}
}