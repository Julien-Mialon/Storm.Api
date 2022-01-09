using Storm.Api.Core.Logs;

namespace Storm.Api.Core.Extensions;

public static class LogServiceExtensions
{
	public static void Debug(this ILogService service, Action<IObjectWriter> fillLogEntry)
	{
		service.Log(LogLevel.Debug, fillLogEntry);
	}

	public static void Trace(this ILogService service, Action<IObjectWriter> fillLogEntry)
	{
		service.Log(LogLevel.Trace, fillLogEntry);
	}

	public static void Information(this ILogService service, Action<IObjectWriter> fillLogEntry)
	{
		service.Log(LogLevel.Information, fillLogEntry);
	}

	public static void Warning(this ILogService service, Action<IObjectWriter> fillLogEntry)
	{
		service.Log(LogLevel.Warning, fillLogEntry);
	}

	public static void Error(this ILogService service, Action<IObjectWriter> fillLogEntry)
	{
		service.Log(LogLevel.Error, fillLogEntry);
	}

	public static void Critical(this ILogService service, Action<IObjectWriter> fillLogEntry)
	{
		service.Log(LogLevel.Critical, fillLogEntry);
	}


	public static void Debug(this ILogService service, string message)
	{
		service.Log(LogLevel.Debug, x => x.WriteMessage(message));
	}

	public static void Trace(this ILogService service, string message)
	{
		service.Log(LogLevel.Trace, x => x.WriteMessage(message));
	}

	public static void Information(this ILogService service, string message)
	{
		service.Log(LogLevel.Information, x => x.WriteMessage(message));
	}

	public static void Warning(this ILogService service, string message)
	{
		service.Log(LogLevel.Warning, x => x.WriteMessage(message));
	}

	public static void Error(this ILogService service, string message)
	{
		service.Log(LogLevel.Error, x => x.WriteMessage(message));
	}

	public static void Critical(this ILogService service, string message)
	{
		service.Log(LogLevel.Critical, x => x.WriteMessage(message));
	}
}