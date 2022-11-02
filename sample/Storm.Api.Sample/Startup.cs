using Storm.Api.Extensions;
using Storm.Api.Launchers;
using Storm.Api.Vaults;

namespace Storm.Api.Sample;

public class Startup : BaseStartup
{
	protected override string LogsProjectName { get; } = "Sample";

	public Startup(IConfiguration configuration, IWebHostEnvironment env) : base(configuration, env)
	{
		ForceHttps = false;
	}

	// This method gets called by the runtime. Use this method to add services to the container.
	public override void ConfigureServices(IServiceCollection services)
	{
		base.ConfigureServices(services);
		RegisterSerilogLogger(services);

		Configuration.OnSection("Vault", section => section.LoadVaultConfiguration());

		services.AddControllers();
	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public override void Configure(IApplicationBuilder app, IHostEnvironment env)
	{
		base.Configure(app, env);
	}
}