using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Storm.Api.Configurations;

namespace Storm.Api.Launchers;

public static class DefaultLauncher<TStartup>
	where TStartup : BaseStartup
{
	public static IHostBuilder WebHostBuilder(string[] args,
		Action<IWebHostBuilder>? configureWebHost = null,
		Action<WebHostBuilderContext, IConfigurationBuilder>? configureConfiguration = null,
		Action<KestrelServerOptions>? configureKestrel = null
	) =>
		Host.CreateDefaultBuilder(args)
			.ConfigureWebHostDefaults(webBuilder =>
			{
				webBuilder
					.ConfigureAppConfiguration((context, configurationBuilder) =>
					{
						configurationBuilder.LoadConfiguration(context.HostingEnvironment);
						configureConfiguration?.Invoke(context, configurationBuilder);
					})
					.ConfigureKestrel(x =>
					{
						x.Limits.MaxRequestBodySize = 2_000_000_000;
						x.Limits.MaxRequestLineSize = 10_000_000;
						x.Limits.MaxRequestBufferSize = 10_000_000;
						x.Limits.MaxRequestHeadersTotalSize = 10_000_000;
						configureKestrel?.Invoke(x);
					})
					.UseStartup<TStartup>();
				configureWebHost?.Invoke(webBuilder);
			});

	public static IHost BuildWebHost(string[] args,
		Action<IWebHostBuilder>? configureWebHost = null,
		Action<WebHostBuilderContext, IConfigurationBuilder>? configureConfiguration = null,
		Action<KestrelServerOptions>? configureKestrel = null
	) => WebHostBuilder(args, configureWebHost, configureConfiguration, configureKestrel).Build();

	public static void RunWebHost(string[] args,
		Action<IWebHostBuilder>? configureWebHost = null,
		Action<WebHostBuilderContext, IConfigurationBuilder>? configureConfiguration = null,
		Action<KestrelServerOptions>? configureKestrel = null
	) => BuildWebHost(args, configureWebHost, configureConfiguration, configureKestrel).Run();
}