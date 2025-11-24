using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Storm.Api.Vaults;

namespace Storm.Api.Launchers;

public static class DefaultLauncherOptions
{
	public static bool SetDatabaseDebug { get; set; }
	public static bool UseVault { get; set; }
	public static bool UseNewtonsoftJson { get; set; }
	public static bool UseOldMigrations { get; set; }
}

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
						if (DefaultLauncherOptions.UseVault)
						{
							configurationBuilder.AddJsonFile("vault.local.json", optional: true, reloadOnChange: false)
								.AddVault();
						}

						if (DefaultLauncherOptions.SetDatabaseDebug)
						{
							configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
							{
								["Database:debugLogging"] = "true",
								["Database:useLogService"] = "false"
							});
						}

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