using System.Runtime.CompilerServices;
using Storm.Api.SourceGenerators.Tests.Infrastructure;
using Xunit;

namespace Storm.Api.SourceGenerators.Tests;

/// <summary>
/// Covers the OpenAPI metadata the generator lifts from attributes on the action class: summary,
/// description, error codes, HTTP error codes, media type and the <c>[InternalActionCall]</c>
/// propagation. Each test compares the complete generated controller against its committed snapshot.
/// </summary>
public class OpenApiMetadataTests
{
	/// <summary>Builds a controller whose single action carries <paramref name="actionAttributes"/>.</summary>
	private static GeneratorOutput Generate(string actionAttributes)
	{
		string source = $$"""

			[ErrorCode("InnerCode", Description = "inner")]
			[HttpError(HttpStatusCode.Unauthorized)]
			[Summary("Inner summary")]
			[Description("Inner description")]
			public class Inner : BaseAction<Unit, string>
			{
				public Inner(IServiceProvider services) : base(services) { }
				protected override Task<string> Action(Unit parameter) => Task.FromResult("");
			}

			{{actionAttributes}}
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
		return GeneratorTestHelper.Run(Sources.InNamespace(source));
	}

	private static void Match(string actionAttributes, [CallerMemberName] string testName = "")
		=> Snapshot.MatchController(Generate(actionAttributes), "C", testName: testName);

	[Fact]
	public void No_metadata_attributes_emits_only_the_default_success_response()
		=> Match("");

	[Fact]
	public void Summary_becomes_an_EndpointSummaryAttribute()
		=> Match("""[Summary("A nice summary")]""");

	[Fact]
	public void Description_becomes_an_EndpointDescriptionAttribute()
		=> Match("""[Description("Some explanation")]""");

	[Fact]
	public void Multiple_descriptions_are_concatenated()
		=> Match("[Description(\"first part\")]\n[Description(\"second part\")]");

	[Fact]
	public void MediaType_overrides_the_success_content_type()
		=> Match("""[MediaType("text/csv")]""");

	[Fact]
	public void Error_codes_appear_in_the_description_and_in_the_OpenApiErrorCodes_attribute()
		=> Match("[ErrorCode(\"Alpha\", Description = \"a desc\")]\n[ErrorCode(\"Beta\")]");

	[Fact]
	public void Error_codes_in_the_OpenApiErrorCodes_attribute_are_sorted_and_deduplicated()
		=> Match("[ErrorCode(\"Zulu\")]\n[ErrorCode(\"Alpha\")]\n[ErrorCode(\"Alpha\")]");

	[Fact]
	public void Duplicate_error_code_descriptions_are_joined()
		=> Match("[ErrorCode(\"Same\", Description = \"one\")]\n[ErrorCode(\"Same\", Description = \"two\")]");

	[Fact]
	public void HttpError_becomes_a_problem_json_ProducesResponseType()
		=> Match("""[HttpError(HttpStatusCode.BadRequest, Description = "bad input")]""");

	[Fact]
	public void Multiple_http_errors_emit_one_attribute_each()
		=> Match("[HttpError(HttpStatusCode.BadRequest)]\n[HttpError(HttpStatusCode.Forbidden)]");

	[Fact]
	public void InternalActionCall_propagates_error_codes_and_http_errors_but_not_summary_or_description()
		=> Match("[ErrorCode(\"OuterError\")]\n[InternalActionCall<Inner>]");

	[Fact]
	public void SuccessCode_overrides_the_default_200_success_response()
		=> Match("""[SuccessCode(HttpStatusCode.Created)]""");

	[Fact]
	public void SuccessCode_with_a_description_documents_it_on_the_success_response()
		=> Match("""[SuccessCode(HttpStatusCode.Accepted, Description = "queued for processing")]""");

	[Fact]
	public void Multiple_success_codes_emit_one_success_response_each()
		=> Match("[SuccessCode(HttpStatusCode.OK)]\n[SuccessCode(HttpStatusCode.Created)]");
}
