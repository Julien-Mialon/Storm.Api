using Storm.Api.SourceGenerators.Tests.Infrastructure;
using Xunit;

namespace Storm.Api.SourceGenerators.Tests;

/// <summary>
/// Covers <c>ContextTransformer.CreateArguments</c> and the parameter-mapping output. Each test
/// compares the complete generated controller against its committed snapshot.
/// </summary>
public class ParameterMappingTests
{
	[Fact]
	public void Parameter_is_auto_mapped_to_a_property_with_the_same_name()
	{
		const string SOURCE = """

			public class P { public required string Name { get; init; } }
			public class Q : BaseAction<P, string>
			{
				public Q(IServiceProvider services) : base(services) { }
				protected override Task<string> Action(P parameter) => Task.FromResult("");
			}
			public partial class C : BaseController
			{
				public C(IServiceProvider services) : base(services) { }
				[WithAction<Q>] public partial Task<ActionResult<Response<string>>> M([FromRoute] string name);
			}

			""";
		Snapshot.MatchController(GeneratorTestHelper.Run(Sources.InNamespace(SOURCE)), "C");
	}

	[Fact]
	public void MapTo_overrides_the_target_property_name()
	{
		const string SOURCE = """

			public class P { public required int A { get; init; } public required int B { get; init; } }
			public class Q : BaseAction<P, int>
			{
				public Q(IServiceProvider services) : base(services) { }
				protected override Task<int> Action(P parameter) => Task.FromResult(0);
			}
			public partial class C : BaseController
			{
				public C(IServiceProvider services) : base(services) { }
				[WithAction<Q>] public partial Task<ActionResult<Response<int>>> M(
					[FromQuery, MapTo(nameof(P.A))] int first,
					[FromQuery, MapTo(nameof(P.B))] int second);
			}

			""";
		Snapshot.MatchController(GeneratorTestHelper.Run(Sources.InNamespace(SOURCE)), "C");
	}

	[Fact]
	public void Auto_map_matches_by_type_when_names_differ()
	{
		// 'payload' has no same-named property; it is matched to the single property of its type (Data).
		const string SOURCE = """

			public class Body { public int Id { get; init; } }
			public class P { public required Body Data { get; init; } }
			public class Q : BaseAction<P, Unit>
			{
				public Q(IServiceProvider services) : base(services) { }
				protected override Task<Unit> Action(P parameter) => Task.FromResult(Unit.Default);
			}
			public partial class C : BaseController
			{
				public C(IServiceProvider services) : base(services) { }
				[WithAction<Q>] public partial Task<ActionResult<Response>> M([FromBody] Body payload);
			}

			""";
		Snapshot.MatchController(GeneratorTestHelper.Run(Sources.InNamespace(SOURCE)), "C");
	}

	[Fact]
	public void Unmappable_parameter_is_dropped_from_both_the_signature_and_the_initializer()
	{
		// 'extra' is a string but the parameter type exposes no string property, so it cannot be
		// mapped. NOTE: CreateArguments keeps only mapped arguments, so an unmappable parameter is
		// dropped from the generated method signature entirely. The generated code therefore no longer
		// matches the user's partial declaration (it won't compile), which is why assertCompiles:false
		// is used here. The snapshot documents exactly what is emitted.
		const string SOURCE = """

			public class P { public required int Id { get; init; } }
			public class Q : BaseAction<P, Unit>
			{
				public Q(IServiceProvider services) : base(services) { }
				protected override Task<Unit> Action(P parameter) => Task.FromResult(Unit.Default);
			}
			public partial class C : BaseController
			{
				public C(IServiceProvider services) : base(services) { }
				[WithAction<Q>] public partial Task<ActionResult<Response>> M([FromRoute] int id, [FromQuery] string extra);
			}

			""";
		Snapshot.MatchController(GeneratorTestHelper.Run(Sources.InNamespace(SOURCE)), "C", assertCompiles: false);
	}

	[Fact]
	public void No_parameters_with_Unit_action_parameter_uses_Unit_Default()
	{
		const string SOURCE = """

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
		Snapshot.MatchController(GeneratorTestHelper.Run(Sources.InNamespace(SOURCE)), "C");
	}

	[Fact]
	public void No_mappable_parameters_with_non_Unit_parameter_uses_object_initializer_new()
	{
		const string SOURCE = """

			public class P { public int X { get; init; } }
			public class Q : BaseAction<P, string>
			{
				public Q(IServiceProvider services) : base(services) { }
				protected override Task<string> Action(P parameter) => Task.FromResult("");
			}
			public partial class C : BaseController
			{
				public C(IServiceProvider services) : base(services) { }
				[WithAction<Q>] public partial Task<ActionResult<Response<string>>> M();
			}

			""";
		Snapshot.MatchController(GeneratorTestHelper.Run(Sources.InNamespace(SOURCE)), "C");
	}

	[Fact]
	public void Case_insensitive_name_match_is_preferred_over_other_same_typed_properties()
	{
		// Both A and B are int; the parameter named "b" must bind to B, not A.
		const string SOURCE = """

			public class P { public required int A { get; init; } public required int B { get; init; } }
			public class Q : BaseAction<P, int>
			{
				public Q(IServiceProvider services) : base(services) { }
				protected override Task<int> Action(P parameter) => Task.FromResult(0);
			}
			public partial class C : BaseController
			{
				public C(IServiceProvider services) : base(services) { }
				[WithAction<Q>] public partial Task<ActionResult<Response<int>>> M([FromQuery] int a, [FromQuery] int b);
			}

			""";
		Snapshot.MatchController(GeneratorTestHelper.Run(Sources.InNamespace(SOURCE)), "C");
	}
}
