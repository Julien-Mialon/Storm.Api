using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Storm.Api.SourceGenerators.ActionMethods.Contexts;
using Storm.Api.SourceGenerators.Bases;
using Storm.Api.SourceGenerators.Bases.Contexts;

namespace Storm.Api.SourceGenerators.ActionMethods;

[Generator(LanguageNames.CSharp)]
public class ActionMethodCodeGenerator : BaseCodeGenerator
{
	public static string GeneratedNamespace => typeof(ActionMethodCodeGenerator).Namespace ?? string.Empty;

	protected override List<AttributeDefinition> Attributes { get; } =
	[
		ActionMethodConstants.WITH_ACTION_ATTRIBUTE,
		ActionMethodConstants.MAP_TO_ATTRIBUTE,
		ActionMethodConstants.SUCCESS_CODE_ATTRIBUTE,
		ActionMethodConstants.MEDIA_TYPE_ATTRIBUTE,
		ActionMethodConstants.ERROR_CODE_ATTRIBUTE,
		ActionMethodConstants.HTTP_ERROR_ATTRIBUTE,
		ActionMethodConstants.DESCRIPTION_ATTRIBUTE,
		ActionMethodConstants.SUMMARY_ATTRIBUTE,
		ActionMethodConstants.INTERNAL_ACTION_CALL_ATTRIBUTE,
	];

	public override void Initialize(IncrementalGeneratorInitializationContext context)
	{
		base.Initialize(context);

		IncrementalValuesProvider<(ActionMethodContext? context, DiagnosticContext? diagnostics)> methodProvider = context.SyntaxProvider
			.ForAttributeWithMetadataName(
				ActionMethodConstants.WITH_ACTION_ATTRIBUTE.MetadataName,
				static (node, _) => node is MethodDeclarationSyntax, //TODO: no more check on partial class
				CreateSemanticContext);

		context.RegisterSourceOutput(methodProvider.Where(static x => x.diagnostics is { Items.Length: > 0 })
				.Select(static (x, _) => x.diagnostics!.Value)
				.WithComparer(EqualityComparer<DiagnosticContext>.Default),
			GenerateDiagnostics);

		// FAWMN fires once per decorated method; regroup them back into one partial class per type
		// (also merges methods declared across multiple files of the same partial class).
		IncrementalValuesProvider<GeneratorContext> classProvider = methodProvider
			.Where(static x => x.context is not null)
			.Select(static (x, _) => x.context!.Value)
			.Collect()
			.SelectMany(static (methods, _) => GroupByClass(methods));

		context.RegisterSourceOutput(classProvider.WithComparer(EqualityComparer<GeneratorContext>.Default), GenerateCode);
	}

	private (ActionMethodContext? context, DiagnosticContext? diagnostics) CreateSemanticContext(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
	{
		ContextTransformer transformer = new(context);
		return transformer.Transform(cancellationToken);
	}

	private static IEnumerable<GeneratorContext> GroupByClass(ImmutableArray<ActionMethodContext> methods)
	{
		foreach (IGrouping<(string? Namespace, string ClassName), ActionMethodContext> group in methods.GroupBy(static x => (x.Namespace, x.ClassName)))
		{
			ActionMethodContext first = group.First();
			yield return new GeneratorContext
			{
				Namespace = first.Namespace,
				ClassName = first.ClassName,
				ClassAccessibility = first.ClassAccessibility,
				Types = first.Types,
				Methods = group
					.OrderBy(static x => x.Method.Name, StringComparer.Ordinal)
					.ThenBy(static x => x.Method.ActionType, StringComparer.Ordinal)
					.Select(static x => x.Method)
					.ToImmutableArray(),
			};
		}
	}

	private void GenerateCode(SourceProductionContext sourceContext, GeneratorContext context)
	{
		CodeGenerator codeGenerator = new(context, GeneratedCodeAttribute);
		string fullText = codeGenerator.Generate();

		sourceContext.AddSource($"{context.Namespace ?? "global"}.{context.ClassName}.Storm.Api.ActionMethods.g.cs", fullText);
	}
}