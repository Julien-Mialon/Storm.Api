using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Storm.Api.SourceGenerators.Bases;

internal abstract class BaseContextTransformer<TContext> where TContext : struct
{
	protected GeneratorSyntaxContext Context { get; }

	protected List<DiagnosticItemContext> Diagnostics { get; } = [];

	protected BaseContextTransformer(GeneratorSyntaxContext context)
	{
		Context = context;
	}

	public (TContext? context, DiagnosticContext? diagnostics) Transform(CancellationToken ct)
	{
		try
		{
			TContext? context = CreateContext(ct);
			return (context, CreateDiagnosticContext());
		}
		catch (Exception ex)
		{
			Diagnostics.Add(new()
			{
				Id = "SG0001",
				Location = Context.Node.GetLocation(),
				MessageFormat = ex.Message,
				Severity = DiagnosticSeverity.Error,
				Title = "Exception while generating code",
			});
		}

		return (null, CreateDiagnosticContext());
	}

	private DiagnosticContext? CreateDiagnosticContext()
	{
		if (Diagnostics.Count == 0)
		{
			return null;
		}

		return new()
		{
			Items = Diagnostics.ToImmutableList()
		};
	}

	protected abstract TContext? CreateContext(CancellationToken ct);

	protected void Log(DiagnosticSeverity severity, string title, string message, string id)
	{
		Diagnostics.Add(new()
		{
			Id = id,
			Location = Context.Node.GetLocation(),
			MessageFormat = message,
			Severity = severity,
			Title = title,
		});
	}

	protected void Error(string title, string message, string id = "SG0001") => Log(DiagnosticSeverity.Error, title, message, id);
	protected void Warning(string title, string message, string id = "SG0001") => Log(DiagnosticSeverity.Warning, title, message, id);
	protected void Info(string title, string message, string id = "SG0001") => Log(DiagnosticSeverity.Info, title, message, id);
	protected void Debug(string title, string message, string id = "SG0001") => Log(DiagnosticSeverity.Hidden, title, message, id);

	protected INamedTypeSymbol GetRequiredTypeByMetadataName(string fullyQualifiedTypeName)
	{
		return Context.SemanticModel.Compilation.GetTypeByMetadataName(fullyQualifiedTypeName)
		       ?? throw new Exception($"Type {fullyQualifiedTypeName} not found");
	}

	protected INamedTypeSymbol GetInterfaceIAction() => GetRequiredTypeByMetadataName("Storm.Api.CQRS.IAction`2");
	protected INamedTypeSymbol GetTypeTask() => GetRequiredTypeByMetadataName("System.Threading.Tasks.Task");
	protected INamedTypeSymbol GetTypeTaskT() => GetRequiredTypeByMetadataName("System.Threading.Tasks.Task`1");
	protected INamedTypeSymbol GetTypeUnit() => GetRequiredTypeByMetadataName("Storm.Api.Unit");
	protected INamedTypeSymbol GetTypeResponse() => GetRequiredTypeByMetadataName("Storm.Api.Dtos.Response");
	protected INamedTypeSymbol GetTypeResponseT() => GetRequiredTypeByMetadataName("Storm.Api.Dtos.Response`1");
	protected INamedTypeSymbol GetTypeApiFileResult() => GetRequiredTypeByMetadataName("Storm.Api.CQRS.Domains.Results.ApiFileResult");
	protected INamedTypeSymbol GetAspNetTypeFileResult() => GetRequiredTypeByMetadataName("Microsoft.AspNetCore.Mvc.FileResult");
	protected INamedTypeSymbol GetAspNetInterfaceIActionResult() => GetRequiredTypeByMetadataName("Microsoft.AspNetCore.Mvc.IActionResult");
	protected INamedTypeSymbol GetAspNetTypeActionResultT() => GetRequiredTypeByMetadataName("Microsoft.AspNetCore.Mvc.ActionResult`1");

	protected bool TryGetAttribute(ISymbol symbol, INamedTypeSymbol attributeType, [NotNullWhen(true)] out AttributeData? attributeData)
	{
		if (attributeType.IsGenericType)
		{
			attributeData = symbol.GetAttributes().FirstOrDefault(x => x.AttributeClass is { IsGenericType: true } && SymbolEqualityComparer.Default.Equals(x.AttributeClass.ConstructedFrom, attributeType));
		}
		else
		{
			attributeData = symbol.GetAttributes().FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attributeType));
		}

		return attributeData is not null;
	}

	protected bool TryGetGenericInterface(ITypeSymbol type, INamedTypeSymbol genericInterfaceType, [NotNullWhen(true)] out INamedTypeSymbol? implementedInterfaceType)
	{
		if (type.Interfaces is { Length: > 0 } interfaces)
		{
			foreach (INamedTypeSymbol interfaceType in interfaces)
			{
				if (interfaceType.IsGenericType && SymbolEqualityComparer.Default.Equals(genericInterfaceType, interfaceType.ConstructedFrom))
				{
					implementedInterfaceType = interfaceType;
					return true;
				}
			}
		}

		if (type.BaseType is {} baseType)
		{
			return TryGetGenericInterface(baseType, genericInterfaceType, out implementedInterfaceType);
		}

		implementedInterfaceType = null;
		return false;
	}

	protected bool Inherits(ITypeSymbol type, INamedTypeSymbol baseType)
	{
		if (SymbolEqualityComparer.Default.Equals(type, baseType))
		{
			return true;
		}

		if (type.BaseType is {} parentType)
		{
			return Inherits(parentType, baseType);
		}

		return false;
	}
}