using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Storm.Api.SourceGenerators.Tests.Infrastructure;

/// <summary>Everything produced by a single generator run, with convenience accessors for assertions.</summary>
internal sealed class GeneratorOutput
{
	public GeneratorOutput(Compilation outputCompilation, ImmutableArray<Diagnostic> generatorDiagnostics, GeneratorDriverRunResult runResult)
	{
		OutputCompilation = outputCompilation;
		GeneratorDiagnostics = generatorDiagnostics;
		RunResult = runResult;
	}

	/// <summary>The input compilation with the generated sources added.</summary>
	public Compilation OutputCompilation { get; }

	/// <summary>Diagnostics reported by the generator itself (e.g. the SG0001 catch-all).</summary>
	public ImmutableArray<Diagnostic> GeneratorDiagnostics { get; }

	public GeneratorDriverRunResult RunResult { get; }

	/// <summary>All sources emitted by the generator (post-init attribute definitions + controllers).</summary>
	public IReadOnlyList<(string HintName, string Text)> GeneratedSources => RunResult.Results
		.SelectMany(result => result.GeneratedSources)
		.Select(source => (source.HintName, source.SourceText.ToString()))
		.ToList();

	/// <summary>Errors raised when compiling the input + generated code together.</summary>
	public ImmutableArray<Diagnostic> CompilationErrors => OutputCompilation
		.GetDiagnostics()
		.Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
		.ToImmutableArray();

	/// <summary>The generated partial-class body for the given controller (fails loudly if absent).</summary>
	public string GeneratedControllerFor(string controllerClassName)
	{
		string marker = $".{controllerClassName}{GeneratorTestHelper.GeneratedHintSuffix}";
		(string HintName, string Text) match = GeneratedSources
			.FirstOrDefault(source => source.HintName.EndsWith(marker, StringComparison.Ordinal));

		if (match.HintName is null)
		{
			string emitted = string.Join(", ", GeneratedSources.Select(source => source.HintName));
			throw new InvalidOperationException(
				$"No generated source for controller '{controllerClassName}'. Emitted hints: [{emitted}].");
		}

		return match.Text;
	}

	/// <summary>True when none of the generated controllers were emitted.</summary>
	public bool HasAnyControllerOutput => GeneratedSources
		.Any(source => source.HintName.EndsWith(GeneratorTestHelper.GeneratedHintSuffix, StringComparison.Ordinal));
}
