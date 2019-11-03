using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Storm.Api.Core.Logs;
using Storm.Api.Logs.Appenders;

namespace Storm.Api.Middlewares
{
	public static class RequestLoggingMiddleware
	{
		public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
		{
			return app.Use((context, next) =>
			{
				ILogService logService = context.RequestServices.GetService<ILogService>();

				logService.Log(LogLevel.Information, x => x
					.WriteProperty("route", context.Request.Path.ToString())
					.WriteProperty("method", context.Request.Method)
					.WriteProperty("device_id", context.Request.Headers.GetOrDefault("X-Device-Id", "undefined"))
					.WriteProperty("type", "HTTP_LOG")
					.WriteRequestContext(context)
					.WriteRequestHeader(context)
				);

				return next();
			});
		}

		public static string GetOrDefault(this IHeaderDictionary headers, string key, string defaultValue = default)
		{
			if (headers.TryGetValue(key, out StringValues value))
			{
				return value.ToString();
			}

			return defaultValue;
		}
	}
}