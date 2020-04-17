using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Storm.Api.Swaggers
{
	public static class SwaggerMiddleware
	{
		private static SwaggerDocumentDescription[] _swaggerDocumentDescriptions;

		public static IServiceCollection AddStormSwagger(this IServiceCollection services, IWebHostEnvironment environment, params SwaggerDocumentDescription[] documentDescription)
		{
			_swaggerDocumentDescriptions = documentDescription;

			return services.AddSwaggerGen(options =>
			{
				options.CustomSchemaIds(x => x.FullName);
				//options.DocumentFilter<SortByNameFilter>();
				//options.OperationFilter<HandleIFormFileFilter>();
				options.OperationFilter<OperationDescriptionFilter>();
				options.DocInclusionPredicate((version, apiDescription) => _swaggerDocumentDescriptions.Any(x => x.InclusionPredicate(version, apiDescription)));

				foreach (SwaggerDocumentDescription apiVersionDoc in _swaggerDocumentDescriptions)
				{
					apiVersionDoc.Apply(options);
				}

				foreach (string xmlDocumentationFile in _swaggerDocumentDescriptions.SelectMany(x => x.DocumentationFiles).Distinct())
				{
					string file = new[]
					{
						$@"{environment.ContentRootPath}{Path.DirectorySeparatorChar}{xmlDocumentationFile}",
						$@"{environment.ContentRootPath}{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}Release{Path.DirectorySeparatorChar}netcoreapp3.0{Path.DirectorySeparatorChar}{xmlDocumentationFile}",
						$@"{environment.ContentRootPath}{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}Debug{Path.DirectorySeparatorChar}netcoreapp3.0{Path.DirectorySeparatorChar}{xmlDocumentationFile}",
					}.FirstOrDefault(File.Exists);

					if (file != default)
					{
						options.IncludeXmlComments(file);
					}
				}
			});
		}

		public static IApplicationBuilder UseStormSwagger(this IApplicationBuilder app)
		{
			return app
				.UseSwagger()
				.UseSwaggerUI(options =>
				{
					foreach (SwaggerDocumentDescription apiVersionDoc in _swaggerDocumentDescriptions)
					{
						apiVersionDoc.Apply(options);
					}
				});
		}
	}
}