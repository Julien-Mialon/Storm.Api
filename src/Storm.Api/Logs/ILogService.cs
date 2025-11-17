using Microsoft.Extensions.Logging;
using Storm.Api.Logs.Interfaces;

namespace Storm.Api.Logs;

public interface ILogService
{
	void Log(LogLevel level, Action<IObjectWriter> fillLogEntry);

	ILogService WithAppender(ILogAppender appender);

	ILogService WithAppenders(params ILogAppender[] appender);
}