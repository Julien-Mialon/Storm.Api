using Storm.Api.SourceGenerators.Tests.Infrastructure;
using Xunit;

namespace Storm.Api.SourceGenerators.Tests;

/// <summary>
/// Covers <c>ActionMethodCodeGenerator.GroupByClass</c>: how per-method results are regrouped into a
/// single partial class, including ordering, multi-file partials, accessibility and namespaces. Each
/// test compares the complete generated controller against its committed snapshot; the snapshot also
/// captures the deterministic, ordinal-by-name method ordering.
/// </summary>
public class ClassGroupingTests
{
	private const string ACTIONS = """

		public class Q1 : BaseAction<Unit, string>
		{
			public Q1(IServiceProvider services) : base(services) { }
			protected override Task<string> Action(Unit parameter) => Task.FromResult("");
		}
		public class Q2 : BaseAction<Unit, int>
		{
			public Q2(IServiceProvider services) : base(services) { }
			protected override Task<int> Action(Unit parameter) => Task.FromResult(0);
		}

		""";

	[Fact]
	public void Multiple_methods_are_grouped_into_one_class_and_ordered_by_name()
	{
		// Declared Zeta-then-Alpha; the snapshot must show a single partial class with Alpha first.
		const string SOURCE = ACTIONS + """

		public partial class C : BaseController
		{
			public C(IServiceProvider services) : base(services) { }
			[WithAction<Q1>] public partial Task<ActionResult<Response<string>>> Zeta();
			[WithAction<Q2>] public partial Task<ActionResult<Response<int>>> Alpha();
		}

		""";
		Snapshot.MatchController(GeneratorTestHelper.Run(Sources.InNamespace(SOURCE)), "C");
	}

	[Fact]
	public void Partial_class_split_across_files_is_merged_into_one_output()
	{
		string fileA = ACTIONS + """

		public partial class C : BaseController
		{
			public C(IServiceProvider services) : base(services) { }
			[WithAction<Q1>] public partial Task<ActionResult<Response<string>>> FromFileA();
		}

		""";
		string fileB = """

			using System;
			using System.Threading.Tasks;
			using Microsoft.AspNetCore.Mvc;
			using Storm.Api.Controllers;
			using Storm.Api.Dtos;
			using Storm.Api.SourceGenerators.ActionMethods;

			namespace Sample;

			public partial class C
			{
				[WithAction<Q2>] public partial Task<ActionResult<Response<int>>> FromFileB();
			}

			""";
		GeneratorOutput output = GeneratorTestHelper.RunFiles(Sources.InNamespace(fileA), fileB);

		// Exactly one generated file for controller C (no duplicate hint names across the two partials).
		int controllerOutputs = output.GeneratedSources
			.Count(generatedSource => generatedSource.HintName.EndsWith(".C" + GeneratorTestHelper.GeneratedHintSuffix, StringComparison.Ordinal));
		Assert.Equal(1, controllerOutputs);

		// ...and the single output merges the methods from both files.
		Snapshot.MatchController(output, "C");
	}

	[Fact]
	public void Internal_controller_emits_an_internal_partial_class_and_methods()
	{
		const string SOURCE = ACTIONS + """

		internal partial class C : BaseController
		{
			public C(IServiceProvider services) : base(services) { }
			[WithAction<Q1>] internal partial Task<ActionResult<Response<string>>> M();
		}

		""";
		Snapshot.MatchController(GeneratorTestHelper.Run(Sources.InNamespace(SOURCE)), "C");
	}

	[Fact]
	public void Controller_in_global_namespace_emits_no_namespace_declaration()
	{
		// No namespace wrapper here.
		const string SOURCE = Sources.USINGS + ACTIONS + """

		public partial class C : BaseController
		{
			public C(IServiceProvider services) : base(services) { }
			[WithAction<Q1>] public partial Task<ActionResult<Response<string>>> M();
		}

		""";
		Snapshot.MatchController(GeneratorTestHelper.Run(SOURCE), "C");
	}
}
