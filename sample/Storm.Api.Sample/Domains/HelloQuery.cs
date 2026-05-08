using System.Net;
using System.Text.Json.Serialization;
using Storm.Api.CQRS;
using Storm.Api.CQRS.Domains.Results;
using Storm.Api.Dtos;
using Storm.Api.Extensions;
using Storm.Api.SourceGenerators.ActionMethods;

namespace Storm.Api.Sample.Domains;

public static class Constants
{
	public const string SHORT_CODE1 = "ShortCode1";
	public const string DESC1 = "short description";
	public const string SUMMARY1 = "Query summary";
}

public class HelloQueryParameter
{
	public required string Name { get; init; }
}

[ErrorCode("ShortCode1")]
[ErrorCode("ShortCode2", Description = "short description")]
[ErrorCode("LongCode1", Description = "This is a very long description for the error code, and it should be split into multiple lines. Let's see how it looks like. Does it look good? I hope it does.")]
[HttpError(HttpStatusCode.BadRequest, Description = "Some dev didn't work correctly")]
[HttpError(HttpStatusCode.Unauthorized, Description = "You shouldn't pass")]
[HttpError(HttpStatusCode.Forbidden, Description = "You don't have permission to access this resource")]
[Summary("Query summary")]
[Description("--- Additional description for the query. ---\n\nThis should also be split into multiple lines if it's too long. Let's see how it looks like.")]
public class InUnitQuery(IServiceProvider services) : BaseAction<Unit, string>(services)
{
	protected override Task<string> Action(Unit parameter)
	{
		return "Hello".AsTask();
	}
}

[ErrorCode(Constants.SHORT_CODE1, Description = Constants.DESC1)]
[Summary(Constants.SUMMARY1)]
[Description(Constants.DESC1)]
[InternalActionCall<InUnitQuery>]
[HttpError(HttpStatusCode.BadRequest, Description = "Hmm that's weird")]
public class HelloQuery(IServiceProvider services) : BaseAction<HelloQueryParameter, HelloResponse>(services)
{
	protected override Task<HelloResponse> Action(HelloQueryParameter parameter)
	{
		return new HelloResponse
		{
			Greetings = $"Hello {parameter.Name}"
		}.AsTask();
	}
}

public class HelloResponse
{
	[JsonPropertyName("greetings")]
	public required string Greetings { get; set; }
}

public class SumQueryParameter
{
	public required int A { get; init; }

	public required int B { get; init; }
}

public class SumQuery(IServiceProvider services) : BaseAction<SumQueryParameter, int>(services)
{
	protected override Task<int> Action(SumQueryParameter parameter)
	{
		return (parameter.A + parameter.B).AsTask();
	}
}

public class RawQueryParameter { }

public class RawQuery(IServiceProvider services) : BaseAction<RawQueryParameter, Response>(services)
{
	protected override Task<Response> Action(RawQueryParameter parameter)
	{
		return new Response().AsTask();
	}
}

public class RawTQuery(IServiceProvider services) : BaseAction<RawQueryParameter, Response<int>>(services)
{
	protected override Task<Response<int>> Action(RawQueryParameter parameter)
	{
		return new Response<int>().AsTask();
	}
}

[MediaType("image/png")]
public class FileQuery(IServiceProvider services) : BaseAction<RawQueryParameter, ApiFileResult>(services)
{
	protected override Task<ApiFileResult> Action(RawQueryParameter parameter)
	{
		throw new NotImplementedException();
	}
}

[ErrorCode("ErrorShort")]
public class UnitQuery(IServiceProvider services) : BaseAuthenticatedAction<RawQueryParameter, Unit, Unit>(services)
{
	protected override Task<Unit> Action(RawQueryParameter parameter, Unit account)
	{
		return Unit.Default.AsTask();
	}
}

public class AutoMapRequest
{
	public int Id { get; init; }

	public required string Name { get; init; }
}

public class AutoMapQueryParameter
{
	public DateTime? Date { get; init; }

	public required AutoMapRequest Data { get; init; }

	public int Id { get; init; }
}

[ErrorCode("Error1", Description = "short description")]
public class AutoMapQuery(IServiceProvider services) : BaseAction<AutoMapQueryParameter, Unit>(services)
{
	protected override Task<Unit> Action(AutoMapQueryParameter parameter)
	{
		return Unit.Default.AsTask();
	}
}