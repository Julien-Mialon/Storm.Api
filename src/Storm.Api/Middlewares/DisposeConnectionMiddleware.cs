using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Core.Databases;

namespace Storm.Api.Middlewares;

internal static class DisposeConnectionMiddleware
{
	public static IApplicationBuilder UseDisposeDatabaseServiceMiddleware(this IApplicationBuilder app)
	{
		return app.Use(async (context, next) =>
		{
			await next();

			context.RequestServices
				.GetService<IDatabaseService>()
				?.Dispose();
		});
	}
}