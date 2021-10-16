using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Storm.Api.Swaggers.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Storm.Api.Swaggers
{
	internal class OperationDescriptionFilter : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			if (context.ApiDescription.TryGetMethodInfo(out MethodInfo methodInfo))
			{
				List<string> lines = new List<string>();

				lines.AddRange(methodInfo.GetCustomAttributes<ImplementationNotesAttribute>().Select(x => x.Description));

				List<ErrorCodeAttribute> errorCodes = methodInfo.GetCustomAttributes<ErrorCodeAttribute>().ToList();
				if (errorCodes.Count > 0)
				{
					if (lines.Count > 0)
					{
						lines.Add(string.Empty);
					}

					lines.Add("Error codes : ");
					lines.AddRange(errorCodes.Select(x => $"  - {x.ErrorCode}: {x.Explanation}"));
				}

				operation.Description = string.Join(Environment.NewLine, lines);

				CategoryAttribute category = methodInfo.GetCustomAttribute<CategoryAttribute>();
				if (category is not null)
				{
					operation.Tags = new List<OpenApiTag>
					{
						new()
						{
							Name = category.Category
						}
					};
				}
			}
		}
	}
}