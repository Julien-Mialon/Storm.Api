using System.Net;
using Storm.Api.CQRS.Exceptions;

namespace Storm.Api.Tests.CQRS;

public class DomainHttpCodeExceptionTests
{
	[Fact]
	public void Constructor_CodeOnly_ConvertsHttpStatusToInt()
	{
		DomainHttpCodeException ex = new(HttpStatusCode.NotFound);
		ex.Code.Should().Be(404);
	}

	[Fact]
	public void Constructor_CodeOnly_UsesCodeToStringAsErrorCode()
	{
		DomainHttpCodeException ex = new(HttpStatusCode.NotFound);
		ex.ErrorCode.Should().Be("NotFound");
	}

	[Fact]
	public void Constructor_CodeOnly_UsesEmptyStringAsErrorMessage()
	{
		DomainHttpCodeException ex = new(HttpStatusCode.NotFound);
		ex.ErrorMessage.Should().Be("");
	}

	[Fact]
	public void Constructor_CodeAndMessage_SetsErrorMessage()
	{
		DomainHttpCodeException ex = new(HttpStatusCode.BadRequest, "bad");
		ex.ErrorMessage.Should().Be("bad");
		ex.ErrorCode.Should().Be("BadRequest");
	}

	[Fact]
	public void Constructor_CodeAndErrorCodeAndMessage_SetsBoth()
	{
		DomainHttpCodeException ex = new(HttpStatusCode.BadRequest, "CUSTOM", "msg");
		ex.ErrorCode.Should().Be("CUSTOM");
		ex.ErrorMessage.Should().Be("msg");
	}

	[Fact]
	public void Constructor_WithInner_PreservesInnerException()
	{
		Exception inner = new("i");
		DomainHttpCodeException ex = new(HttpStatusCode.InternalServerError, "msg", inner);
		ex.InnerException.Should().BeSameAs(inner);
	}

	[Theory]
	[InlineData(HttpStatusCode.BadRequest, 400)]
	[InlineData(HttpStatusCode.Unauthorized, 401)]
	[InlineData(HttpStatusCode.Forbidden, 403)]
	[InlineData(HttpStatusCode.NotFound, 404)]
	[InlineData(HttpStatusCode.InternalServerError, 500)]
	public void Code_MapsToExpectedInteger(HttpStatusCode status, int expected)
	{
		new DomainHttpCodeException(status).Code.Should().Be(expected);
	}
}
