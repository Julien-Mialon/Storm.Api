using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Storm.Api.SourceGenerators.Bases.Contexts;

namespace Storm.Api.SourceGenerators.Bases;

internal abstract class BaseContextTransformer<TContext> where TContext : struct
{
	protected SemanticModel SemanticModel { get; }

	protected Location DefaultLocation { get; }

	protected List<DiagnosticItemContext> Diagnostics { get; } = [];

	protected BaseContextTransformer(SemanticModel semanticModel, Location defaultLocation)
	{
		SemanticModel = semanticModel;
		DefaultLocation = defaultLocation;
	}

	protected BaseContextTransformer(GeneratorSyntaxContext context)
	{
		SemanticModel = context.SemanticModel;
		DefaultLocation = context.Node.GetLocation();
	}

	protected BaseContextTransformer(GeneratorAttributeSyntaxContext context)
	{
		SemanticModel = context.SemanticModel;
		DefaultLocation = context.TargetNode.GetLocation();
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
				Location = new(DefaultLocation),
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
			Items = Diagnostics.ToImmutableArray(),
		};
	}

	protected abstract TContext? CreateContext(CancellationToken ct);

	protected void Log(DiagnosticSeverity severity, string title, string message, string id)
	{
		Diagnostics.Add(new()
		{
			Id = id,
			Location = new(DefaultLocation),
			MessageFormat = message,
			Severity = severity,
			Title = title,
		});
	}

	protected void Error(string title, string message, string id = "SG0001")
		=> Log(DiagnosticSeverity.Error, title, message, id);

	protected void Warning(string title, string message, string id = "SG0001")
		=> Log(DiagnosticSeverity.Warning, title, message, id);

	protected void Info(string title, string message, string id = "SG0001")
		=> Log(DiagnosticSeverity.Info, title, message, id);

	protected void Debug(string title, string message, string id = "SG0001")
		=> Log(DiagnosticSeverity.Hidden, title, message, id);

	protected static bool TryGetAttribute(ISymbol symbol, INamedTypeSymbol attributeType, [NotNullWhen(true)] out AttributeData? attributeData)
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

	protected static bool HasAttribute(ISymbol symbol, INamedTypeSymbol attributeType)
	{
		if (attributeType.IsGenericType)
		{
			return symbol.GetAttributes().Any(x => x.AttributeClass is { IsGenericType: true } && SymbolEqualityComparer.Default.Equals(x.AttributeClass.ConstructedFrom, attributeType));
		}

		return symbol.GetAttributes().Any(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attributeType));
	}

	protected static IEnumerable<AttributeData> GetAttributes(ISymbol symbol, INamedTypeSymbol attributeType)
	{
		if (attributeType.IsGenericType)
		{
			return symbol.GetAttributes().Where(x => x.AttributeClass is { IsGenericType: true } && SymbolEqualityComparer.Default.Equals(x.AttributeClass.ConstructedFrom, attributeType));
		}

		return symbol.GetAttributes().Where(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, attributeType));
	}

	protected static bool TryGetGenericInterface(ITypeSymbol type, INamedTypeSymbol genericInterfaceType, [NotNullWhen(true)] out INamedTypeSymbol? implementedInterfaceType)
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

		if (type.BaseType is { } baseType)
		{
			return TryGetGenericInterface(baseType, genericInterfaceType, out implementedInterfaceType);
		}

		implementedInterfaceType = null;
		return false;
	}

	protected static bool Inherits(ITypeSymbol type, INamedTypeSymbol baseType)
	{
		if (SymbolEqualityComparer.Default.Equals(type, baseType))
		{
			return true;
		}

		if (type.BaseType is { } parentType)
		{
			return Inherits(parentType, baseType);
		}

		return false;
	}

	protected static bool IsGenericTypeInstance(ITypeSymbol type, INamedTypeSymbol genericType)
	{
		if (SymbolEqualityComparer.Default.Equals(type, genericType))
		{
			return true;
		}

		if (type is INamedTypeSymbol namedType)
		{
			return namedType.IsGenericType && SymbolEqualityComparer.Default.Equals(namedType.ConstructedFrom, genericType);
		}

		return false;
	}
}