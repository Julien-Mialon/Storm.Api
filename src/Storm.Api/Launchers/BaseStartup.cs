using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Storm.Api.Configurations;
using Storm.Api.Core.Databases;
using Storm.Api.Core.Logs;
using Storm.Api.Core.Logs.Appenders;
using Storm.Api.Core.Logs.Consoles;
using Storm.Api.Core.Logs.ElasticSearch.Configurations;
using Storm.Api.Core.Logs.Serilogs.Configurations;
using Storm.Api.Core.Services;
using Storm.Api.Databases;
using Storm.Api.Extensions;
using Storm.Api.Logs.Appenders;
using Storm.Api.Middlewares;
using Storm.Api.Services;
using Storm.Api.Swaggers;

namespace Storm.Api.Launchers
{
	public abstract class BaseStartup
	{
		protected IConfiguration Configuration { get; }
		protected IWebHostEnvironment Environment { get; }
		protected ILogService LogService { get; private set; }

		protected abstract string LogsProjectName { get; }
		protected virtual SwaggerDocumentDescription[] SwaggerDocuments => new SwaggerDocumentDescription[]
		{
			new SwaggerDocumentDescription("v1", new SwaggerModuleDescription("API", ""))
		};

		public BaseStartup(IConfiguration configuration, IWebHostEnvironment environment)
		{
			Environment = environment;
			Configuration = configuration;
		}

		public virtual void ConfigureServices(IServiceCollection services)
		{
			Configuration.OnSection("Features", FeatureFlagsHelper.Load);

			services.AddSingleton<IDateService, DateService>()
				.AddSingleton<IScopeServiceAccessor, ScopeServiceAccessor>();

			Configuration.OnSection("Database", section => services.AddDatabaseModule(section.LoadDatabaseConfiguration()));


			// frameworks
			services.AddMvc()
				.AddNewtonsoftJson(options =>
				{
					options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
				});
			services.AddCors();
			services.AddStormSwagger(Environment, SwaggerDocuments);
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

			services.Configure<FormOptions>(x =>
			{
				x.ValueLengthLimit = 1024 * 1024 * 1024; //1GB
				x.MultipartBodyLengthLimit = 1024 * 1024 * 1024; //1GB
			});

			services.Configure<RequestLocalizationOptions>(options =>
			{
				CultureInfo[] supportedCultures = new[]
				{
					new CultureInfo("fr")
				};
				options.DefaultRequestCulture = new RequestCulture(culture: "fr", uiCulture: "fr");
				options.SupportedCultures = supportedCultures;
				options.SupportedUICultures = supportedCultures;
			});
		}

		public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (!EnvironmentHelper.IsAvailableClient)
			{
				app.UseDeveloperExceptionPage();
			}

			IOptions<RequestLocalizationOptions> options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();

			app.UseRouting()
				.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod())
				.EnforceHttps()
				.UseStormSwagger()
				.UseDatabaseModule()
				.UseRequestLogging()
				.UseRequestLocalization(options.Value)
				.UseEndpoints(endpoints => endpoints.MapControllers())
				;

			LogService?.WithAppender(new RequestContextAppender(app.ApplicationServices.GetService<IActionContextAccessor>()))
				.WithAppender(new RequestHeaderAppender(app.ApplicationServices.GetService<IActionContextAccessor>()))
				;
		}

		protected void RegisterElasticSearchLogger(IServiceCollection services, string configurationSectionName = "ElasticSearch")
		{
			SetLogService(services, ElasticSearchConfiguration.CreateBuilder()
					.WithMinimumLogLevel(LogLevel.Debug)
					.WithIndex($"{LogsProjectName}-api-{Environment.EnvironmentName}", "log")
					.WithQueueSender()
					.FromConfiguration(Configuration.GetSection(configurationSectionName))
					.Build()
					.CreateService()
					.WithAppender(new TimestampLogAppender())
				);
		}

		protected void RegisterSerilogLogger(IServiceCollection services, string configurationSectionName = "Serilog")
		{
			SetLogService(services, SerilogConfiguration.CreateBuilder()
					.WithMinimumLogLevel(LogLevel.Debug)
					.FromConfiguration(Configuration.GetSection(configurationSectionName))
					.Build()
					.CreateService()
					.WithAppender(new TimestampLogAppender())
				);
		}

		protected void RegisterConsoleLogger(IServiceCollection services, LogLevel minimumLogLevel = LogLevel.Debug)
		{
			SetLogService(services, new LogService(s => new ConsoleLogSender(), minimumLogLevel));
		}

		protected void SetLogService(IServiceCollection services, ILogService logService)
		{
			LogService = logService;
			services.AddSingleton(logService);
			LogServiceDatabaseLog.LogService = logService;
		}
	}
}