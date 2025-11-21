using Storm.Api.CQRS;
using Storm.Api.CQRS.Domains.Results;
using Storm.Api.Dtos;
using Storm.Api.Extensions;

namespace Storm.Api.Sample.Domains;

public class HelloQueryParameter
{
	public required string Name { get; init; }
}

public class HelloQuery(IServiceProvider services) : BaseAction<HelloQueryParameter, string>(services)
{
	protected override Task<string> Action(HelloQueryParameter parameter)
	{
		return $"Hello {parameter.Name}".AsTask();
	}
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

public class RawQueryParameter
{

}

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

public class FileQuery(IServiceProvider services) : BaseAction<RawQueryParameter, ApiFileResult>(services)
{
	protected override Task<ApiFileResult> Action(RawQueryParameter parameter)
	{
		throw new NotImplementedException();
	}
}

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

public class AutoMapQuery(IServiceProvider services) : BaseAction<AutoMapQueryParameter, Unit>(services)
{
	protected override Task<Unit> Action(AutoMapQueryParameter parameter)
	{
		return Unit.Default.AsTask();
	}
}