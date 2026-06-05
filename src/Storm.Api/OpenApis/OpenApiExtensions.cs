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

				options.AddDocumentTransformer((document, _, _) =>
				{
					MoveNullSchemaToEnd(document);
					return Task.CompletedTask;
				});

				configureOptions?.Invoke(options);
			});
		}
	}

	private static void MoveNullSchemaToEnd(OpenApiDocument document)
	{
		if (document.Components?.Schemas is null)
		{
			return;
		}

		foreach (IOpenApiSchema schema in document.Components.Schemas.Values)
		{
			MoveNullSchemaToEnd(schema);
		}
	}

	private static void MoveNullSchemaToEnd(IOpenApiSchema schema)
	{
		MoveNullSchemaToEnd(schema.AnyOf);
		MoveNullSchemaToEnd(schema.OneOf);

		if (schema.Properties is null)
		{
			return;
		}

		foreach (IOpenApiSchema propertySchema in schema.Properties.Values)
		{
			MoveNullSchemaToEnd(propertySchema);
		}
	}

	private static void MoveNullSchemaToEnd(IList<IOpenApiSchema>? schemas)
	{
		if (schemas is null || schemas.Count < 2)
		{
			return;
		}

		List<IOpenApiSchema> nullSchemas = schemas
			.Where(x => x.Type == JsonSchemaType.Null)
			.ToList();

		if (nullSchemas.Count == 0)
		{
			return;
		}

		List<IOpenApiSchema> nonNullSchemas = schemas
			.Where(x => x.Type != JsonSchemaType.Null)
			.ToList();

		schemas.Clear();

		foreach (IOpenApiSchema schema in nonNullSchemas.Concat(nullSchemas))
		{
			schemas.Add(schema);
		}
	}
}
