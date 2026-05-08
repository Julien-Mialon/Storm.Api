using System.Text.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Storm.Api.OpenApis;

public static class OpenApiExtensions
{
	extension(IServiceCollection services)
	{
		public IServiceCollection AddStormOpenApi(Action<OpenApiOptions>? configureOptions = null)
		{
			return services.AddOpenApi(options =>
			{
				options.AddOperationTransformer((operation, context, _) =>
				{
					List<OpenApiErrorCodesAttribute> attributes = context.Description.ActionDescriptor.EndpointMetadata
						.OfType<OpenApiErrorCodesAttribute>()
						.ToList();

					if (attributes.Count > 0)
					{
						operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
						operation.Extensions["x-error-codes"] = new JsonNodeExtension(
							JsonSerializer.SerializeToNode(
								attributes.SelectMany(attr => attr.Codes).Distinct().ToArray()
							)!
						);
					}

					return Task.CompletedTask;
				});

				configureOptions?.Invoke(options);
			});
		}
	}
}