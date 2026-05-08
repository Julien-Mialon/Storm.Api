using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Storm.Api.SourceGenerators.Bases;

namespace Storm.Api.SourceGenerators.ActionMethods;

internal class ContextTransformer(GeneratorSyntaxContext context) : BaseContextTransformer<GeneratorContext>(context)
{
	protected override GeneratorContext? CreateContext(CancellationToken cancellationToken)
	{
		ClassDeclarationSyntax classSyntax = (ClassDeclarationSyntax)Context.Node;
		INamedTypeSymbol? classSymbol = Context.SemanticModel.GetDeclaredSymbol(classSyntax, cancellationToken);
		if (classSymbol is null)
		{
			return null;
		}

		Types types = new()
		{
			Unit = GetTypeUnit(),
			Response = GetTypeResponse(),
			ResponseT = GetTypeResponseT(),
			ApiFileResult = GetTypeApiFileResult(),
			IAction = GetInterfaceIAction(),
			OpenApiErrorCodesAttribute = GetOpenApiErrorCodesAttribute(),
			WithActionAttribute = GetRequiredTypeByMetadataName(ActionMethod.WITH_ACTION_ATTRIBUTE.MetadataName),
			MapToAttribute = GetRequiredTypeByMetadataName(ActionMethod.MAP_TO_ATTRIBUTE.MetadataName),
			SuccessCodeAttribute = GetRequiredTypeByMetadataName(ActionMethod.SUCCESS_CODE_ATTRIBUTE.MetadataName),
			ErrorCodeAttribute = GetRequiredTypeByMetadataName(ActionMethod.ERROR_CODE_ATTRIBUTE.MetadataName),
			HttpErrorAttribute = GetRequiredTypeByMetadataName(ActionMethod.HTTP_ERROR_ATTRIBUTE.MetadataName),
			DescriptionAttribute = GetRequiredTypeByMetadataName(ActionMethod.DESCRIPTION_ATTRIBUTE.MetadataName),
			SummaryAttribute = GetRequiredTypeByMetadataName(ActionMethod.SUMMARY_ATTRIBUTE.MetadataName),
			MediaTypeAttribute = GetRequiredTypeByMetadataName(ActionMethod.MEDIA_TYPE_ATTRIBUTE.MetadataName),
			InternalActionCallAttribute = GetRequiredTypeByMetadataName(ActionMethod.INTERNAL_ACTION_CALL_ATTRIBUTE.MetadataName),
			TaskT = GetTypeTaskT(),
			AspNetIActionResult = GetAspNetInterfaceIActionResult(),
			AspNetActionResultT = GetAspNetTypeActionResultT(),
			AspNetFileResult = GetTypeStream(),
			ProducesResponseTypeAttribute = GetAspNetTypeProducesResponseTypeAttribute(),
			EndpointSummaryAttribute = GetAspNetTypeEndpointSummaryAttribute(),
			EndpointDescriptionAttribute = GetAspNetTypeEndpointDescriptionAttribute(),
		};

		List<MethodContext> methodContexts = [];

		foreach (IMethodSymbol methodSymbol in classSymbol.GetMembers().OfType<IMethodSymbol>())
		{
			if (TryGetAttribute(methodSymbol, types.WithActionAttribute, out AttributeData? withActionAttribute) is false)
			{
				continue;
			}

			ITypeSymbol actionType = withActionAttribute.AttributeClass!.TypeArguments[0];
			if (TryGetGenericInterface(withActionAttribute.AttributeClass!.TypeArguments[0], types.IAction, out INamedTypeSymbol? actionInterfaceType) is false)
			{
				continue;
			}

			ITypeSymbol actionParameterType = actionInterfaceType.TypeArguments[0];
			ITypeSymbol actionReturnType = actionInterfaceType.TypeArguments[1];
			(ITypeSymbol methodReturnType, ITypeSymbol openApiReturnType, ActionType type) = AnalyzeReturnType(actionReturnType, types);

			MethodContext methodContext = new()
			{
				Accessibility = methodSymbol.DeclaredAccessibility,
				Name = methodSymbol.Name,
				ReturnType = methodReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
				OpenApiReturnType = openApiReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
				ActionType = actionType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
				ActionParameterType = actionParameterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
				ActionResultType = actionReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
				Arguments = CreateArguments(actionParameterType, methodSymbol, types).ToImmutableList(),
				Type = type,
				ActionResultTypeSymbol = actionReturnType,
				Summary = CreateSummary(actionType, types),
				Description = CreateDescriptionString(actionType, types),
				ErrorCodes = CreateErrorCodes(actionType, types).ToImmutableList(),
				HttpErrorStatusCodes = CreateHttpErrorCodes(actionType, types).ToImmutableList(),
				SuccessStatusCodes = CreateSuccessCodes(actionType, types).ToImmutableList(),
				MediaType = CreateMediaType(actionType, types),
			};

			methodContexts.Add(methodContext);
		}

		return new GeneratorContext
		{
			Namespace = classSymbol.ContainingNamespace.IsGlobalNamespace ? null : classSymbol.ContainingNamespace.ToDisplayString(),
			ClassName = classSymbol.Name,
			ClassAccessibility = classSymbol.DeclaredAccessibility,
			Types = types,
			Methods = methodContexts.ToImmutableList(),
		};
	}

	private (ITypeSymbol methodReturnType, ITypeSymbol openApiReturnType, ActionType type) AnalyzeReturnType(ITypeSymbol actionReturnType, Types types)
	{
		ITypeSymbol methodReturnType;
		ITypeSymbol openApiReturnType;
		ActionType type;
		if (Inherits(actionReturnType, types.ApiFileResult))
		{
			// Task<IActionResult>: we can't use FileResult because it can also return JSON content on error
			methodReturnType = types.TaskT.Construct(types.AspNetIActionResult);
			openApiReturnType = types.AspNetFileResult;
			type = ActionType.File;
		}
		else if (Inherits(actionReturnType, types.Unit))
		{
			// Task<ActionResult<Response>>
			methodReturnType = types.TaskT.Construct(types.AspNetActionResultT.Construct(types.Response));
			openApiReturnType = types.Response;
			type = ActionType.Unit;
		}
		else if (Inherits(actionReturnType, types.Response))
		{
			// Task<ActionResult<TResponse>>
			methodReturnType = types.TaskT.Construct(types.AspNetActionResultT.Construct(actionReturnType));
			openApiReturnType = actionReturnType;
			type = ActionType.Response;
		}
		else
		{
			// Task<ActionResult<Response<TReturnType>>>
			methodReturnType = types.TaskT.Construct(types.AspNetActionResultT.Construct(types.ResponseT.Construct(actionReturnType)));
			openApiReturnType = types.ResponseT.Construct(actionReturnType);
			type = ActionType.Regular;
		}

		return (methodReturnType, openApiReturnType, type);
	}

	private List<MethodArgumentContext> CreateArguments(ITypeSymbol actionParameterType, IMethodSymbol methodSymbol, Types types)
	{
		List<MethodArgumentContext> arguments = [];
		Dictionary<string, IPropertySymbol> parameterProperties = actionParameterType.GetMembers()
			.OfType<IPropertySymbol>()
			.Where(x => x.SetMethod is not null)
			.ToDictionary(x => x.Name, x => x);

		foreach (IParameterSymbol parameter in methodSymbol.Parameters)
		{
			MethodArgumentContext argumentContext = new()
			{
				Name = parameter.Name,
				Type = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
			};

			if (TryGetAttribute(parameter, types.MapToAttribute, out AttributeData? mapToAttribute))
			{
				string? parameterProperty = mapToAttribute.ConstructorArguments[0].Value as string;
				argumentContext.MapTo = parameterProperty;
				if (string.IsNullOrEmpty(parameterProperty) is false)
				{
					parameterProperties.Remove(parameterProperty!);
				}
			}

			arguments.Add(argumentContext);
		}

		List<IPropertySymbol> remainingProperties = parameterProperties.Values.ToList();
		if (remainingProperties.Count > 0)
		{
			for (int index = 0 ; index < arguments.Count ; index++)
			{
				MethodArgumentContext argument = arguments[index];
				if (string.IsNullOrEmpty(argument.MapTo) is false)
				{
					continue;
				}

				IPropertySymbol? pickedProperty = remainingProperties
					.Where(x => x.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == argument.Type)
					.OrderByDescending(x => string.Equals(x.Name, argument.Name, StringComparison.OrdinalIgnoreCase) ? 1 : 0)
					.FirstOrDefault();

				if (pickedProperty is null)
				{
					continue;
				}

				arguments[index] = argument with
				{
					MapTo = pickedProperty.Name,
				};
			}
		}

		return arguments.Where(x => string.IsNullOrEmpty(x.MapTo) is false).ToList();
	}

	private string? CreateSummary(ITypeSymbol actionType, Types types)
	{
		string? result = null;
		if (TryGetAttribute(actionType, types.SummaryAttribute, out AttributeData? summaryAttribute))
		{
			result = summaryAttribute.ConstructorArguments[0].Value as string;
		}

		return result;
	}

	private string? CreateMediaType(ITypeSymbol actionType, Types types)
	{
		string? result = null;
		if (TryGetAttribute(actionType, types.MediaTypeAttribute, out AttributeData? mediaTypeAttribute))
		{
			result = mediaTypeAttribute.ConstructorArguments[0].Value as string;
		}

		return result;
	}

	private string? CreateDescriptionString(ITypeSymbol actionType, Types types)
	{
		string? description = null;

		foreach (AttributeData attributeData in GetAttributes(actionType, types.DescriptionAttribute))
		{
			description ??= "";
			description += $"{attributeData.ConstructorArguments[0].Value}\n\n";
		}

		return description;
	}

	private List<ErrorCodeContext> CreateErrorCodes(ITypeSymbol actionType, Types types)
	{
		List<ErrorCodeContext> result = [];
		foreach (AttributeData attributeData in GetAttributes(actionType, types.ErrorCodeAttribute)
			.Concat(GetAttributesFromInternalActionCall(actionType, types, types.ErrorCodeAttribute)))
		{
			result.Add(new()
			{
				ErrorCode = attributeData.ConstructorArguments[0].Value as string ?? "",
				Description = attributeData.NamedArguments.FirstOrDefault(x => x.Key == "Description").Value.Value as string,
			});
		}

		return result
			.Where(x => string.IsNullOrEmpty(x.ErrorCode) is false)
			.GroupBy(x => x.ErrorCode)
			.Select(x => new ErrorCodeContext
			{
				ErrorCode = x.Key,
				Description = string.Join(" | ", x.Select(y => y.Description).Where(y => y is not null).Distinct()),
			}).ToList();
	}

	private List<StatusCodeContext> CreateHttpErrorCodes(ITypeSymbol actionType, Types types)
	{
		List<StatusCodeContext> result = [];
		foreach (AttributeData attributeData in GetAttributes(actionType, types.HttpErrorAttribute)
			.Concat(GetAttributesFromInternalActionCall(actionType, types, types.HttpErrorAttribute)))
		{
			result.Add(new()
			{
				StatusCode = (int)attributeData.ConstructorArguments[0].Value!,
				Description = attributeData.NamedArguments.FirstOrDefault(x => x.Key == "Description").Value.Value as string,
			});
		}

		return result.GroupBy(x => x.StatusCode)
			.Select(x => new StatusCodeContext
			{
				StatusCode = x.Key,
				Description = string.Join("\n\n", x.Select(y => y.Description).Where(y => y is not null).Distinct()),
			}).ToList();
	}

	private List<StatusCodeContext> CreateSuccessCodes(ITypeSymbol actionType, Types types)
	{
		List<StatusCodeContext> result = [];
		foreach (AttributeData attributeData in GetAttributes(actionType, types.SuccessCodeAttribute))
		{
			result.Add(new()
			{
				StatusCode = (int)attributeData.ConstructorArguments[0].Value!,
				Description = attributeData.NamedArguments.FirstOrDefault(x => x.Key == "Description").Value.Value as string,
			});
		}

		return result;
	}

	private List<AttributeData> GetAttributesFromInternalActionCall(ITypeSymbol actionType, Types types, INamedTypeSymbol attributeType)
	{
		List<AttributeData> internalActionCalls = GetAttributes(actionType, types.InternalActionCallAttribute).ToList();
		List<AttributeData> result = [];
		foreach (AttributeData internalActionCall in internalActionCalls)
		{
			if (internalActionCall.AttributeClass is null)
			{
				continue;
			}

			ITypeSymbol internalActionType = internalActionCall.AttributeClass.TypeArguments[0];
			if (TryGetGenericInterface(internalActionType, types.IAction, out _) is false)
			{
				// not an action
				continue;
			}

			result.AddRange(GetAttributes(internalActionType, attributeType));
		}

		return result;
	}
}