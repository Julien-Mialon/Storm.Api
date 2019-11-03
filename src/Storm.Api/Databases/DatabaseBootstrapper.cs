using Microsoft.AspNetCore.Builder;
using Storm.Api.Middlewares;

namespace Storm.Api.Databases
{
	public static class DatabaseBootstrapper
	{
		public static IApplicationBuilder UseDatabaseModule(this IApplicationBuilder app)
		{
			return app.UseDisposeDatabaseServiceMiddleware();
		}
	}
}