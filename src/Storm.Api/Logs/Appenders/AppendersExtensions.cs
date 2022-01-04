using Microsoft.AspNetCore.Http;
using Storm.Api.Core.Logs;

namespace Storm.Api.Logs.Appenders;

public static class AppendersExtensions
{
	public static IObjectWriter WriteRequestContext(this IObjectWriter logEntry, HttpContext context)
	{
		RequestContextAppender.DumpRequestContext(context, new Dictionary<string, string?>(), logEntry);
		return logEntry;
	}

	public static IObjectWriter WriteRequestHeader(this IObjectWriter logEntry, HttpContext context)
	{
		RequestHeaderAppender.DumpRequestHeader(context, logEntry);
		return logEntry;
	}
}