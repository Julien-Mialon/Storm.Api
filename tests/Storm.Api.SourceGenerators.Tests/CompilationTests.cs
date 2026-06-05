using Storm.Api.SourceGenerators.Tests.Infrastructure;
using Xunit;

namespace Storm.Api.SourceGenerators.Tests;

/// <summary>
/// End-to-end tests: a realistic controller exercising every action type and metadata feature at
/// once must produce code that compiles, and the post-initialization attribute definitions must be
/// emitted so user code can reference them.
/// </summary>
public class CompilationTests
{
	[Theory]
	[InlineData("WithActionAttribute")]
	[InlineData("MapToAttribute")]
	[InlineData("SuccessCodeAttribute")]
	[InlineData("MediaTypeAttribute")]
	[InlineData("ErrorCodeAttribute")]
	[InlineData("HttpErrorAttribute")]
	[InlineData("DescriptionAttribute")]
	[InlineData("SummaryAttribute")]
	[InlineData("InternalActionCallAttribute")]
	public void Marker_attribute_definitions_are_generated(string attributeClassName)
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
		GeneratorOutput output = GeneratorTestHelper.Run(Sources.InNamespace(SOURCE));

		Assert.Contains(
			output.GeneratedSources,
			generatedSource => generatedSource.Text.Contains($"class {attributeClassName}"));
	}

	[Fact]
	public void Generated_marker_attributes_are_internal_and_sealed()
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
		GeneratorOutput output = GeneratorTestHelper.Run(Sources.InNamespace(SOURCE));

		(string HintName, string Text) withAction = Assert.Single(
			output.GeneratedSources,
			generatedSource => generatedSource.Text.Contains("class WithActionAttribute"));
		Assert.Contains("internal sealed class WithActionAttribute", withAction.Text);
	}

	[Fact]
	public void Realistic_controller_with_every_feature_compiles_without_errors()
	{
		const string SOURCE = """

			public class HelloResponse { public required string Greetings { get; set; } }
			public class HelloParameter { public required string Name { get; init; } }
			public class SumParameter { public required int A { get; init; } public required int B { get; init; } }

			[ErrorCode("ShortCode1")]
			[ErrorCode("ShortCode2", Description = "desc 2")]
			[HttpError(HttpStatusCode.BadRequest, Description = "bad")]
			[Summary("Greets the caller")]
			[Description("A friendly greeting endpoint.")]
			[MediaType("application/json")]
			[InternalActionCall<SumQuery>]
			public class HelloQuery : BaseAction<HelloParameter, HelloResponse>
			{
				public HelloQuery(IServiceProvider services) : base(services) { }
				protected override Task<HelloResponse> Action(HelloParameter parameter) => throw new NotImplementedException();
			}

			[ErrorCode("SumError", Description = "sum failed")]
			public class SumQuery : BaseAction<SumParameter, int>
			{
				public SumQuery(IServiceProvider services) : base(services) { }
				protected override Task<int> Action(SumParameter parameter) => Task.FromResult(parameter.A + parameter.B);
			}

			public class RawQuery : BaseAction<HelloParameter, Response>
			{
				public RawQuery(IServiceProvider services) : base(services) { }
				protected override Task<Response> Action(HelloParameter parameter) => Task.FromResult(new Response());
			}

			[MediaType("image/png")]
			public class FileQuery : BaseAction<Unit, ApiFileResult>
			{
				public FileQuery(IServiceProvider services) : base(services) { }
				protected override Task<ApiFileResult> Action(Unit parameter) => throw new NotImplementedException();
			}

			public class AuthedQuery : BaseAuthenticatedAction<SumParameter, Unit, Unit>
			{
				public AuthedQuery(IServiceProvider services) : base(services) { }
				protected override Task<Unit> Action(SumParameter parameter, Unit account) => Task.FromResult(Unit.Default);
			}

			public partial class KitchenSinkController : BaseController
			{
				public KitchenSinkController(IServiceProvider services) : base(services) { }

				[HttpGet("/hello/{name}")]
				[WithAction<HelloQuery>]
				public partial Task<ActionResult<Response<HelloResponse>>> Hello([FromRoute] string name);

				[HttpGet("/sum")]
				[WithAction<SumQuery>]
				public partial Task<ActionResult<Response<int>>> Sum(
					[FromQuery, MapTo(nameof(SumParameter.A))] int a,
					[FromQuery, MapTo(nameof(SumParameter.B))] int b);

				[HttpGet("/raw/{name}")]
				[WithAction<RawQuery>]
				public partial Task<ActionResult<Response>> Raw([FromRoute] string name);

				[HttpGet("/file")]
				[WithAction<FileQuery>]
				public partial Task<IActionResult> File();

				[HttpGet("/authed")]
				[WithAction<AuthedQuery>]
				public partial Task<ActionResult<Response>> Authed([FromQuery] int a, [FromQuery] int b);
			}

			""";
		// Compares the entire generated controller (and asserts it compiles + emits no diagnostics).
		Snapshot.MatchController(GeneratorTestHelper.Run(Sources.InNamespace(SOURCE)), "KitchenSinkController");
	}
}
