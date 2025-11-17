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
			WithActionAttribute = GetRequiredTypeByMetadataName(ActionMethod.WITH_ACTION_ATTRIBUTE.MetadataName),
			MapToAttribute = GetRequiredTypeByMetadataName(ActionMethod.MAP_TO_ATTRIBUTE.MetadataName),
			TaskT = GetTypeTaskT(),
			AspNetIActionResult = GetAspNetInterfaceIActionResult(),
			AspNetActionResultT = GetAspNetTypeActionResultT(),
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
			ITypeSymbol methodReturnType;
			ActionType type;

			if (Inherits(actionReturnType, types.ApiFileResult))
			{
				// Task<IActionResult>: we can't use FileResult because it can also return JSON content on error
				methodReturnType = types.TaskT.Construct(types.AspNetIActionResult);
				type = ActionType.File;
			}
			else if (Inherits(actionReturnType, types.Unit))
			{
				// Task<ActionResult<Response>>
				methodReturnType = types.TaskT.Construct(
					types.AspNetActionResultT.Construct(
						types.Response));
				type = ActionType.Unit;
			}
			else if (Inherits(actionReturnType, types.Response))
			{
				// Task<ActionResult<TResponse>>
				methodReturnType = types.TaskT.Construct(
					types.AspNetActionResultT.Construct(
						actionReturnType));
				type = ActionType.Response;
			}
			else
			{
				// Task<ActionResult<Response<TReturnType>>>
				methodReturnType = types.TaskT.Construct(
					types.AspNetActionResultT.Construct(
						types.ResponseT.Construct(actionReturnType)));
				type = ActionType.Regular;
			}

			List<MethodArgumentContext> arguments = [];

			foreach (IParameterSymbol parameter in methodSymbol.Parameters)
			{
				MethodArgumentContext argumentContext = new()
				{
					Name = parameter.Name,
					Type = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
				};

				if (TryGetAttribute(parameter, types.MapToAttribute, out AttributeData? mapToAttribute))
				{
					argumentContext.MapTo = mapToAttribute.ConstructorArguments[0].Value as string;
				}

				arguments.Add(argumentContext);
			}

			MethodContext methodContext = new()
			{
				Accessibility = methodSymbol.DeclaredAccessibility,
				Name = methodSymbol.Name,
				ReturnType = methodReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
				ActionType = actionType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
				ActionParameterType = actionParameterType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
				ActionResultType = actionReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
				Arguments = arguments.ToImmutableList(),
				Type = type,
				ActionResultTypeSymbol = actionReturnType,
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
}