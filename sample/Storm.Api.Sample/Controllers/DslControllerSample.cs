using Microsoft.AspNetCore.Mvc;
using Storm.Api.Controllers;
using Storm.Api.Dtos;
using Storm.Api.Sample.Domains;
using Storm.Api.SourceGenerators.ActionMethods;

namespace Storm.Api.Sample.Controllers;

public partial class DslControllerSample(IServiceProvider services) : BaseController(services)
{
	[HttpGet("/hello-clean")]
	[WithAction<InUnitQuery>]
	[Tags("Hello")]
	public partial Task<ActionResult<Response<string>>> HelloWorld();

	[HttpGet("/hello/{name}")]
	[WithAction<HelloQuery>]
	[Tags("Hello")]
	public partial Task<ActionResult<Response<HelloResponse>>> HelloWorld([FromRoute] string name);

	[HttpGet("/ids/{id}")]
	[WithAction<AutoMapQuery>]
	[Tags("Hello")]
	public partial Task<ActionResult<Response>> HelloWorld([FromRoute] int id, [FromBody] AutoMapRequest request, [FromHeader(Name = "X-Date")] DateTime? date);

	[HttpGet("/hello2/{name}")]
	[WithAction<SumQuery>]
	[Tags("Hello")]
	public partial Task<ActionResult<Response<int>>> HelloWorldA([FromQuery(Name = "a"), MapTo(nameof(SumQueryParameter.A))] int a,
		[FromQuery(Name = "b"), MapTo(nameof(SumQueryParameter.B))] int b);

	[HttpGet("/hello2.1/{name}")]
	[WithAction<SumQuery>]
	[Tags("Hello")]
	public partial Task<ActionResult<Response<int>>> HelloWorldANoMap([FromQuery(Name = "a")] int a,
		[FromQuery(Name = "b")] int b);

	[HttpGet("/hello3/{name}")]
	[WithAction<RawQuery>]
	public partial Task<ActionResult<Response>> HelloWorldB();

	[HttpGet("/hello4/{name}")]
	[WithAction<RawTQuery>]
	public partial Task<ActionResult<Response<int>>> HelloWorldC();

	[HttpGet("/hello5/{name}")]
	[WithAction<FileQuery>]
	public partial Task<IActionResult> HelloWorldD();

	[HttpGet("/hello6/{name}")]
	[WithAction<UnitQuery>]
	public partial Task<ActionResult<Response>> HelloWorldE();
}

public static class Urls
{
	public const string POST_SUM = "/sum";
	public const string GET_SUM_EXTENDED = "/sum-extended/{b}";
}