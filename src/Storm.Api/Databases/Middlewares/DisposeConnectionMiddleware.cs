using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Databases.Services;

namespace Storm.Api.Databases.Middlewares;

public static class DisposeConnectionMiddleware
{
	public static IApplicationBuilder UseDisposeConnectionMiddleware(this IApplicationBuilder app)
	{
		return app.Use(async (context, next) =>
		{
			await next();

			foreach (IDatabaseService databaseService in context.RequestServices.GetServices<IDatabaseService>())
			{
				databaseService.Dispose();
			}
		});
	}
}