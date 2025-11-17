using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Logs;
using Storm.Api.Logs.Extensions;

namespace Storm.Api.Middlewares;

public static class RequestLoggingMiddleware
{
	public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
	{
		return app.Use((context, next) =>
		{
			ILogService logService = context.RequestServices.GetRequiredService<ILogService>();

			logService.Information(x => x
				.WriteProperty("filter", "HTTP_LOG")
				.WriteProperty("route", context.Request.Path.ToString())
				.WriteProperty("method", context.Request.Method)
				.WriteRequestContext(context)
			);

			return next();
		});
	}
}