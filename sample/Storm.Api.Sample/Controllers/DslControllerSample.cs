using Microsoft.AspNetCore.Mvc;
using Storm.Api.Controllers;
using Storm.Api.Dtos;
using Storm.Api.Sample.Domains;
using Storm.Api.SourceGenerators.ActionMethods;

namespace Storm.Api.Sample.Controllers;

public partial class DslControllerSample(IServiceProvider services) : BaseController(services)
{
	[HttpGet("/hello/{name}")]
	[WithAction<SumQuery>]
	public partial Task<ActionResult<Response<int>>> HelloWorldA(
		[FromQuery(Name = "a"), MapTo(nameof(SumQueryParameter.A))] int a,
		[FromQuery(Name = "b"), MapTo(nameof(SumQueryParameter.B))] int b
		);

	[HttpGet("/hello3/{name}")]
	[WithAction<RawQuery>]
	public partial Task<ActionResult<Response>> HelloWorldB();

	[HttpGet("/hello4/{name}")]
	[WithAction<RawTQuery>]
	[ProducesResponseType(statusCode: 400, type: typeof(Response))]
	[ProducesResponseType(statusCode: 401, type: typeof(Response))]
	public partial Task<ActionResult<Response<int>>> HelloWorldC();

	[HttpGet("/hello5/{name}")]
	[WithAction<FileQuery>]
	[ProducesResponseType<FileResult>(statusCode: 200, contentType: "image/jpeg")]
	public partial Task<IActionResult> HelloWorldD();

	[HttpGet("/hello6/{name}")]
	[WithAction<UnitQuery>]
	[ProducesResponseType<Response>(400)]
	[ProducesResponseType<Response>(401)]
	public partial Task<ActionResult<Response>> HelloWorldE();
}


public static class Urls
{
	public const string POST_SUM = "/sum";
	public const string GET_SUM_EXTENDED = "/sum-extended/{b}";
}