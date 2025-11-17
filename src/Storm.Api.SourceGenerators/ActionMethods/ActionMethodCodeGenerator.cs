using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Storm.Api.SourceGenerators.Bases;

namespace Storm.Api.SourceGenerators.ActionMethods;

[Generator(LanguageNames.CSharp)]
public class ActionMethodCodeGenerator : BaseCodeGenerator
{
	protected override List<AttributeDefinition> Attributes { get; } = [ActionMethod.WITH_ACTION_ATTRIBUTE, ActionMethod.MAP_TO_ATTRIBUTE];

	public static string GeneratedNamespace => typeof(ActionMethodCodeGenerator).Namespace ?? string.Empty;

	public override void Initialize(IncrementalGeneratorInitializationContext context)
	{
		base.Initialize(context);

		IncrementalValuesProvider<(GeneratorContext? context, DiagnosticContext? diagnostics)> syntaxProvider = context.SyntaxProvider
			.CreateSyntaxProvider(CouldBeAClassToGenerate, CreateSemanticContext);

		context.RegisterSourceOutput(
			syntaxProvider.Where(static x => x.diagnostics is { Items.Count: > 0 })
				.Select(static (x, _) => x.diagnostics!.Value)
				.WithComparer(EqualityComparer<DiagnosticContext>.Default),
			GenerateDiagnostics);

		context.RegisterSourceOutput(
			syntaxProvider.Where(static x => x.context is not null)
				.Select(static (x, _) => x.context!.Value)
				.WithComparer(EqualityComparer<GeneratorContext>.Default),
			GenerateCode);
	}

	private bool CouldBeAClassToGenerate(SyntaxNode node, CancellationToken cancellationToken)
	{
		if (node is not ClassDeclarationSyntax classDeclarationSyntax)
		{
			return false;
		}

		return classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword)
		       && classDeclarationSyntax.Members.Any(member =>
			       member is MethodDeclarationSyntax
			       && member.AttributeLists.Count > 0
			       && member.AttributeLists.Any(attributes =>
				       attributes.Attributes.Any(attribute => attribute.Name.ToString().Contains(ActionMethod.WITH_ACTION_ATTRIBUTE.Name))
			       )
		       );
	}

	private (GeneratorContext? context, DiagnosticContext? diagnostics) CreateSemanticContext(GeneratorSyntaxContext context, CancellationToken cancellationToken)
	{
		ContextTransformer transformer = new(context);
		return transformer.Transform(cancellationToken);
	}

	private void GenerateCode(SourceProductionContext sourceContext, GeneratorContext context)
	{
		CodeGenerator codeGenerator = new(context, GeneratedCodeAttribute);
		string fullText = codeGenerator.Generate();

		sourceContext.AddSource($"{context.Namespace ?? "global"}.{context.ClassName}.Storm.Api.ActionMethods.g.cs", fullText);
	}
}