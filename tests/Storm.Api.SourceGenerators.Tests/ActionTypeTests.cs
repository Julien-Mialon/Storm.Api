using Storm.Api.SourceGenerators.Tests.Infrastructure;
using Xunit;

namespace Storm.Api.SourceGenerators.Tests;

/// <summary>
/// Covers the four shapes <c>ContextTransformer.AnalyzeReturnType</c> produces from the action's
/// <c>IAction&lt;,&gt;</c> output type: Regular (<c>T</c>), Response, Unit and File. Each test compares
/// the complete generated controller against its committed snapshot.
/// </summary>
public class ActionTypeTests
{
	[Fact]
	public void Regular_output_is_wrapped_in_ActionResult_of_Response_of_T()
	{
		const string SOURCE = """

			public class P { public int X { get; init; } }
			public class Q : BaseAction<P, int>
			{
				public Q(IServiceProvider services) : base(services) { }
				protected override Task<int> Action(P parameter) => Task.FromResult(0);
			}
			public partial class C : BaseController
			{
				public C(IServiceProvider services) : base(services) { }
				[WithAction<Q>] public partial Task<ActionResult<Response<int>>> M();
			}

			""";
		Snapshot.MatchController(GeneratorTestHelper.Run(Sources.InNamespace(SOURCE)), "C");
	}

	[Fact]
	public void Response_output_is_returned_directly_without_a_wrapper_payload()
	{
		const string SOURCE = """

			public class P { public int X { get; init; } }
			public class Q : BaseAction<P, Response>
			{
				public Q(IServiceProvider services) : base(services) { }
				protected override Task<Response> Action(P parameter) => Task.FromResult(new Response());
			}
			public partial class C : BaseController
			{
				public C(IServiceProvider services) : base(services) { }
				[WithAction<Q>] public partial Task<ActionResult<Response>> M();
			}

			""";
		Snapshot.MatchController(GeneratorTestHelper.Run(Sources.InNamespace(SOURCE)), "C");
	}

	[Fact]
	public void Response_of_T_action_output_keeps_the_generic_payload_type()
	{
		const string SOURCE = """

			public class P { public int X { get; init; } }
			public class Q : BaseAction<P, Response<int>>
			{
				public Q(IServiceProvider services) : base(services) { }
				protected override Task<Response<int>> Action(P parameter) => Task.FromResult(new Response<int>());
			}
			public partial class C : BaseController
			{
				public C(IServiceProvider services) : base(services) { }
				[WithAction<Q>] public partial Task<ActionResult<Response<int>>> M();
			}

			""";
		Snapshot.MatchController(GeneratorTestHelper.Run(Sources.InNamespace(SOURCE)), "C");
	}

	[Fact]
	public void Unit_output_produces_a_bare_Response_with_IsSuccess()
	{
		const string SOURCE = """

			public class P { public int X { get; init; } }
			public class Q : BaseAction<P, Unit>
			{
				public Q(IServiceProvider services) : base(services) { }
				protected override Task<Unit> Action(P parameter) => Task.FromResult(Unit.Default);
			}
			public partial class C : BaseController
			{
				public C(IServiceProvider services) : base(services) { }
				[WithAction<Q>] public partial Task<ActionResult<Response>> M();
			}

			""";
		Snapshot.MatchController(GeneratorTestHelper.Run(Sources.InNamespace(SOURCE)), "C");
	}

	[Fact]
	public void File_output_uses_FileAction_and_returns_IActionResult()
	{
		const string SOURCE = """

			public class Q : BaseAction<Unit, ApiFileResult>
			{
				public Q(IServiceProvider services) : base(services) { }
				protected override Task<ApiFileResult> Action(Unit parameter) => throw new NotImplementedException();
			}
			public partial class C : BaseController
			{
				public C(IServiceProvider services) : base(services) { }
				[WithAction<Q>] public partial Task<IActionResult> M();
			}

			""";
		Snapshot.MatchController(GeneratorTestHelper.Run(Sources.InNamespace(SOURCE)), "C");
	}
}
