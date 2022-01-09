using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Storm.Api.Configurations;

namespace Storm.Api.Launchers;

public static class DefaultLauncher<TStartup>
	where TStartup : BaseStartup
{
	public static IHostBuilder WebHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
			.ConfigureWebHostDefaults(webBuilder => webBuilder
				.ConfigureAppConfiguration((context, configurationBuilder) => configurationBuilder.LoadConfiguration(context.HostingEnvironment))
				.ConfigureKestrel(x =>
				{
					x.Limits.MaxRequestBodySize = 2_000_000_000;
					x.Limits.MaxRequestLineSize = 10_000_000;
					x.Limits.MaxRequestBufferSize = 10_000_000;
					x.Limits.MaxRequestHeadersTotalSize = 10_000_000;
				})
				.UseStartup<TStartup>()
			);

	public static IHost BuildWebHost(string[] args) => WebHostBuilder(args).Build();

	public static void RunWebHost(string[] args) => BuildWebHost(args).Run();
}