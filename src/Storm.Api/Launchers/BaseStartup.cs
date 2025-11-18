using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Storm.Api.Databases;
using Storm.Api.Databases.Configurations;
using Storm.Api.Databases.Migrations;
using Storm.Api.Databases.Migrations.Models;
using Storm.Api.Extensions;
using Storm.Api.Features;
using Storm.Api.Logs;
using Storm.Api.Logs.Extensions;
using Storm.Api.Logs.Sinks.Consoles;
using Storm.Api.Logs.Sinks.ElasticSearch.Configurations;
using Storm.Api.Logs.Sinks.ElasticSearch.HostedServices;
using Storm.Api.Logs.Sinks.ElasticSearch.Senders;
using Storm.Api.Logs.Sinks.Serilogs.Configurations;
using Storm.Api.Middlewares;
using Storm.Api.Servers;
using Storm.Api.Services;

namespace Storm.Api.Launchers;

public abstract class BaseStartup
{
	private IMigrationModule[] _migrationModules =
	[
	];

	protected IConfiguration Configuration { get; }
	protected ServerConfiguration ServerConfiguration { get; }
	protected IHostEnvironment Environment { get; }
	protected ILogService LogService { get; private set; }
	protected bool WaitForMigrationsBeforeStarting { get; set; } = true;

	protected BaseStartup(IConfiguration configuration, IHostEnvironment environment)
	{
		Environment = environment;
		Configuration = configuration;
		LogService = new LogService(_ => new ConsoleLogSink(), LogLevel.Warning);

		ServerConfiguration = Configuration.WithSection("Server", c => c.LoadServerConfiguration())
		                      ?? ServerConfigurationExtensions.DEFAULT_SERVER_CONFIGURATION;

		AppContext.SetSwitch("System.Net.Http.EnableActivityPropagation", false);
		DistributedContextPropagator.Current = DistributedContextPropagator.CreateNoOutputPropagator();
	}

	public virtual void ConfigureServices(IServiceCollection services)
	{
		Configuration.OnSection("Features", FeatureFlags.LoadFromConfiguration);

		services.AddSingleton<IDateService, DateService>()
			.AddSingleton<IScopedServiceAccessor, ScopedServiceAccessor>()
			.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

		// frameworks
		services.AddMvc().AddJsonLibrary();
		services.AddCors();
		services.AddOpenApi();

		services.Configure<FormOptions>(x =>
		{
			x.ValueLengthLimit = ServerConfiguration.Forms.ValueLengthLimit;
			x.MultipartBodyLengthLimit = ServerConfiguration.Forms.MultipartBodyLengthLimit;
		});

		services.Configure<RequestLocalizationOptions>(options =>
		{
			CultureInfo[] supportedCultures = ServerConfiguration.Cultures.SupportedCultures.Select(x => new CultureInfo(x)).ToArray();
			options.DefaultRequestCulture = new(culture: ServerConfiguration.Cultures.DefaultCulture, uiCulture: ServerConfiguration.Cultures.DefaultCulture);
			options.SupportedCultures = supportedCultures;
			options.SupportedUICultures = supportedCultures;
		});

		Configuration.OnSection("Database", section => services.AddDatabaseModule(section.LoadDatabaseConfiguration()));

		services.AddResponseCompression(options =>
		{
			options.EnableForHttps = true;
			options.Providers.Add<BrotliCompressionProvider>();
			options.Providers.Add<GzipCompressionProvider>();
			options.MimeTypes = ResponseCompressionDefaults.MimeTypes;
			options.ExcludedMimeTypes =
			[
			];
		});

		services.Configure<BrotliCompressionProviderOptions>(options =>
		{
			options.Level = CompressionLevel.Fastest;
		});

		services.Configure<GzipCompressionProviderOptions>(options =>
		{
			options.Level = CompressionLevel.Fastest;
		});

		services.AddControllers();
	}

	public virtual void Configure(IApplicationBuilder app, IHostEnvironment env)
	{
		if (EnvironmentHelper.IsAvailableClient is false)
		{
			app.UseDeveloperExceptionPage();
		}

		IOptions<RequestLocalizationOptions> options = app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>();

		app.UseRouting();
		app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

		if (ServerConfiguration.ForceHttps)
		{
			app.UseHttpsRedirection();
		}

		app.UseDatabaseModule();
		app.UseRequestLogging();
		app.UseRequestLocalization(options.Value);
		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
			if (EnvironmentHelper.IsAvailableClient is false)
			{
				endpoints.MapOpenApi();
				endpoints.MapScalarApiReference();
			}
		});
		if (EnvironmentHelper.IsAvailableClient is false)
		{
			app.UseSwaggerUI(swaggerOptions =>
			{
				swaggerOptions.SwaggerEndpoint("/openapi/v1.json", "API");
			});
		}

		LogService.WithRequestContextAppender(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());

		if (_migrationModules is { Length: > 0 })
		{
			ConfiguredTaskAwaitable<bool> migrationTask = MigrationHelper.Run(app.ApplicationServices, _migrationModules)
				.ConfigureAwait(false);
			if (WaitForMigrationsBeforeStarting)
			{
				migrationTask.GetAwaiter().GetResult();
			}
		}
	}

	protected void RegisterElasticSearchLogger(IServiceCollection services, string configurationSectionName = "ElasticSearch")
	{
		SetLogService(services, Configuration.GetSection(configurationSectionName)
			.LoadElasticSearchConfiguration()
			.WithQueueSink()
			.Build()
			.CreateService());
	}

	protected void RegisterElasticSearchHostedLogger(IServiceCollection services, string configurationSectionName = "ElasticSearch")
	{
		ElasticSearchHostedServiceLogSink logSink = new();
		ElasticSearchConfiguration logConfiguration = Configuration.GetSection(configurationSectionName)
			.LoadElasticSearchConfiguration()
			.WithSink((_, _) => logSink)
			.Build();

		SetLogService(services, logConfiguration.CreateService());
		services.AddSingleton(logConfiguration.CreateElasticSender())
			.AddSingleton(logSink)
			.AddHostedService<ElasticSearchHostedService>();
	}

	protected void RegisterSerilogLogger(IServiceCollection services, string configurationSectionName = "Serilog")
	{
		SetLogService(services, Configuration.GetSection(configurationSectionName)
			.LoadSerilogConfiguration()
			.CreateService()
		);
	}

	protected void RegisterConsoleLogger(IServiceCollection services, LogLevel minimumLogLevel = LogLevel.Debug)
	{
		SetLogService(services, new LogService(_ => new ConsoleLogSink(), minimumLogLevel));
	}

	protected void SetLogService(IServiceCollection services, ILogService logService)
	{
		LogService = logService.WithTimestampAppender();
		services.AddSingleton(logService);
		LogServiceDatabaseLog.LogService = logService;
	}

	protected void UseMigrationModules(params IMigrationModule[] migrationModules)
	{
		_migrationModules = migrationModules;
	}
}