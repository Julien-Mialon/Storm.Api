using Microsoft.CodeAnalysis;
using Storm.Api.SourceGenerators.Tests.Infrastructure;
using Xunit;

namespace Storm.Api.SourceGenerators.Tests;

/// <summary>
/// Covers the incremental pipeline: the value-typed context models must let Roslyn reuse cached
/// output when an edit does not change the semantic shape of a decorated method. This is the
/// property the equality overrides on the context structs exist to provide.
/// </summary>
public class IncrementalCachingTests
{
	private const string V1 = """

		public class Q : BaseAction<Unit, string>
		{
			public Q(IServiceProvider services) : base(services) { }
			protected override Task<string> Action(Unit parameter) => Task.FromResult("");
		}
		public partial class C : BaseController
		{
			public C(IServiceProvider services) : base(services) { }
			[WithAction<Q>] public partial Task<ActionResult<Response<string>>> M();
		}

		""";

	// Same controller, but with an unrelated comment added (changes the syntax tree, not the model).
	private const string V2 = """

		// an unrelated edit that shifts the tree but not the generated output
		public class Q : BaseAction<Unit, string>
		{
			public Q(IServiceProvider services) : base(services) { }
			protected override Task<string> Action(Unit parameter) => Task.FromResult("");
		}
		public partial class C : BaseController
		{
			public C(IServiceProvider services) : base(services) { }
			[WithAction<Q>] public partial Task<ActionResult<Response<string>>> M();
		}

		""";

	[Fact]
	public void Unrelated_edit_keeps_the_generated_output_byte_for_byte_stable()
	{
		(GeneratorDriverRunResult first, GeneratorDriverRunResult second) =
			GeneratorTestHelper.RunIncremental(Sources.InNamespace(V1), Sources.InNamespace(V2));

		Assert.Equal(ControllerText(first), ControllerText(second));
	}

	[Fact]
	public void Unrelated_edit_reuses_the_cached_source_output()
	{
		(_, GeneratorDriverRunResult second) =
			GeneratorTestHelper.RunIncremental(Sources.InNamespace(V1), Sources.InNamespace(V2));

		IReadOnlyList<IncrementalStepRunReason> reasons = second.Results
			.Single()
			.TrackedOutputSteps
			.SelectMany(step => step.Value)
			.SelectMany(runStep => runStep.Outputs)
			.Select(runOutput => runOutput.Reason)
			.ToList();

		Assert.NotEmpty(reasons);
		Assert.All(reasons, reason =>
			Assert.True(
				reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged,
				$"Expected cached/unchanged source output but got '{reason}'."));
	}

	private static string ControllerText(GeneratorDriverRunResult result) => result.Results
		.SelectMany(generatorResult => generatorResult.GeneratedSources)
		.Single(source => source.HintName.EndsWith(".C" + GeneratorTestHelper.GeneratedHintSuffix, StringComparison.Ordinal))
		.SourceText
		.ToString();
}
