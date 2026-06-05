using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Storm.Api.SourceGenerators.ActionMethods;

namespace Storm.Api.SourceGenerators.Tests.Infrastructure;

/// <summary>
/// Drives <see cref="ActionMethodCodeGenerator"/> against an in-memory compilation built from the
/// <see cref="FrameworkStubs"/> plus a per-test controller/action snippet, and exposes the
/// generated sources, generator diagnostics and the post-generation compilation.
/// </summary>
internal static class GeneratorTestHelper
{
	/// <summary>Namespace the generated attributes (<c>WithAction</c>, <c>MapTo</c>, ...) live in.</summary>
	public const string ATTRIBUTES_NAMESPACE = "Storm.Api.SourceGenerators.ActionMethods";

	private const string GENERATED_HINT_SUFFIX = ".Storm.Api.ActionMethods.g.cs";

	private static readonly ImmutableArray<MetadataReference> METADATA_REFERENCES = LoadReferences();

	private static readonly CSharpParseOptions PARSE_OPTIONS =
		new(LanguageVersion.Preview);

	/// <summary>
	/// Runs the generator over <paramref name="userSource"/> (the framework stubs are always
	/// prepended) and returns everything a test might want to assert on.
	/// </summary>
	public static GeneratorOutput Run(string userSource) => RunFiles(userSource);

	/// <summary>
	/// Runs the generator over several user files at once (the framework stubs are always
	/// prepended). Useful for partial classes split across files.
	/// </summary>
	public static GeneratorOutput RunFiles(params string[] userSources)
	{
		ImmutableArray<SyntaxTree> trees = userSources
			.Select(Parse)
			.Prepend(ParseStub())
			.ToImmutableArray();

		return RunOn(BuildCompilation(trees), trackSteps: false);
	}

	/// <summary>Runs the generator against a caller-supplied framework source (no stubs prepended).</summary>
	public static GeneratorOutput RunWithFramework(string frameworkSource, string userSource)
		=> RunOn(BuildCompilation([Parse(frameworkSource), Parse(userSource)]), trackSteps: false);

	/// <summary>
	/// Runs the generator twice, reusing the framework-stub tree and references between runs so the
	/// driver can compute incremental deltas. Only the controller tree changes between
	/// <paramref name="firstSource"/> and <paramref name="secondSource"/>. The result of the second
	/// run is returned so a test can inspect the tracked-step reasons (Cached / Unchanged / Modified).
	/// </summary>
	public static (GeneratorDriverRunResult First, GeneratorDriverRunResult Second) RunIncremental(string firstSource, string secondSource)
	{
		SyntaxTree stub = ParseStub();

		Compilation first = BuildCompilation([stub, Parse(firstSource)]);
		Compilation second = BuildCompilation([stub, Parse(secondSource)]);

		GeneratorDriver driver = CreateDriver(trackSteps: true);
		driver = driver.RunGenerators(first);
		GeneratorDriverRunResult firstResult = driver.GetRunResult();

		driver = driver.RunGenerators(second);
		GeneratorDriverRunResult secondResult = driver.GetRunResult();

		return (firstResult, secondResult);
	}

	private static GeneratorOutput RunOn(Compilation compilation, bool trackSteps)
	{
		GeneratorDriver driver = CreateDriver(trackSteps);
		driver = driver.RunGeneratorsAndUpdateCompilation(
			compilation,
			out Compilation outputCompilation,
			out ImmutableArray<Diagnostic> generatorDiagnostics);

		return new GeneratorOutput(outputCompilation, generatorDiagnostics, driver.GetRunResult());
	}

	private static GeneratorDriver CreateDriver(bool trackSteps)
	{
		ActionMethodCodeGenerator generator = new();
		return CSharpGeneratorDriver.Create(
			[generator.AsSourceGenerator()],
			parseOptions: PARSE_OPTIONS,
			driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackSteps));
	}

	private static Compilation BuildCompilation(ImmutableArray<SyntaxTree> trees)
	{
		return CSharpCompilation.Create(
			"Storm.Api.GeneratorTests.Dynamic",
			trees,
			METADATA_REFERENCES,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
	}

	private static SyntaxTree Parse(string source) => CSharpSyntaxTree.ParseText(source, PARSE_OPTIONS);

	private static SyntaxTree ParseStub() => Parse(FrameworkStubs.SOURCE);

	private static ImmutableArray<MetadataReference> LoadReferences()
	{
		string trusted = (string)(AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? string.Empty);
		return trusted
			.Split(Path.PathSeparator)
			.Where(path => string.IsNullOrEmpty(path) is false)
			.Select(path => (MetadataReference)MetadataReference.CreateFromFile(path))
			.ToImmutableArray();
	}

	/// <summary>The generated controller hint suffix, for filtering the source outputs.</summary>
	public static string GeneratedHintSuffix => GENERATED_HINT_SUFFIX;
}
