using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Storm.Api.Swaggers
{
	public class SwaggerDocumentDescription
	{
		internal string Version { get; }

		internal List<SwaggerModuleDescription> Modules { get; }

		internal List<string> DocumentationFiles { get; } = new List<string>();

		public SwaggerDocumentDescription(string version, params SwaggerModuleDescription[] modules)
		{
			Version = version;
			Modules = modules.ToList();
		}

		public SwaggerDocumentDescription WithXmlDocumentationFile(string fileName)
		{
			DocumentationFiles.Add(fileName);
			return this;
		}

		internal void Apply(SwaggerGenOptions options)
		{
			foreach (SwaggerModuleDescription module in Modules)
			{
				options.SwaggerDoc($"{Version}_{module.ModuleName}", new OpenApiInfo {Title = $"API - {module.ModuleName}", Version = Version});
			}
		}

		internal void Apply(SwaggerUIOptions options)
		{
			foreach (SwaggerModuleDescription module in Modules)
			{
				options.SwaggerEndpoint($"/swagger/{Version}_{module.ModuleName}/swagger.json", $"API {Version} - {module.ModuleName}");
			}
		}

		internal bool InclusionPredicate(string document, ApiDescription description)
		{
			foreach (SwaggerModuleDescription module in Modules)
			{
				if (module.MatchExpressions.Any(x => description.RelativePath.StartsWith(x)))
				{
					return document == $"{Version}_{module.ModuleName}";
				}
			}

			return false;
		}
	}
}