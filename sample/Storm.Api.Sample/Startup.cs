using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceStack.OrmLite;
using Storm.Api.Core.Databases;
using Storm.Api.Launchers;

namespace Storm.Api.Sample
{
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

			services.AddControllers();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public override void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			base.Configure(app, env);
		}
	}
}