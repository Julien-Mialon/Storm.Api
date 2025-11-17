using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Storm.Api.Logs.Appenders;
using Storm.Api.Logs.Interfaces;

namespace Storm.Api.Logs.Extensions;

public static class AppendersExtensions
{
	public static ILogService WithTimestampAppender(this ILogService logService)
	{
		return logService.WithAppender(new TimestampLogAppender());
	}

	public static ILogService WithRequestContextAppender(this ILogService logService, IActionContextAccessor actionContextAccessor)
	{
		return logService.WithAppender(new RequestContextAppender(actionContextAccessor));
	}

	public static IObjectWriter WriteRequestContext(this IObjectWriter logEntry, HttpContext context)
	{
		RequestContextAppender.DumpRequestContext(context, null, logEntry);
		return logEntry;
	}
}