using Storm.Api.SourceGenerators.Tests.Infrastructure;
using Xunit;

namespace Storm.Api.SourceGenerators.Tests;

/// <summary>The simplest end-to-end generation, asserted against a full-output snapshot.</summary>
public class SmokeTests
{
	private const string SOURCE = """

		public class GreetParameter
		{
			public required string Name { get; init; }
		}

		public class GreetQuery : BaseAction<GreetParameter, string>
		{
			public GreetQuery(IServiceProvider services) : base(services) { }
			protected override Task<string> Action(GreetParameter parameter) => Task.FromResult("hi");
		}

		public partial class GreetController : BaseController
		{
			public GreetController(IServiceProvider services) : base(services) { }

			[HttpGet("/greet/{name}")]
			[WithAction<GreetQuery>]
			public partial Task<ActionResult<Response<string>>> Greet([FromRoute] string name);
		}

		""";

	[Fact]
	public void Generates_the_expected_controller()
	{
		// Snapshot.MatchController also asserts there are no generator diagnostics and that the
		// generated code compiles cleanly.
		Snapshot.MatchController(GeneratorTestHelper.Run(Sources.InNamespace(SOURCE)), "GreetController");
	}
}
