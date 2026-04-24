using System.Net;
using Storm.Api.CQRS.Exceptions;
using Storm.Api.CQRS.Extensions;

namespace Storm.Api.Tests.CQRS;

public class ExceptionExtensionsTests
{
	[Fact]
	public void BadRequestIfNull_WithNull_Throws400()
	{
		object? o = null;
		Action act = () => o.BadRequestIfNull();
		act.Should().Throw<DomainHttpCodeException>().Which.Code.Should().Be(400);
	}

	[Fact]
	public void BadRequestIfNull_WithValue_ReturnsValue()
	{
		string? o = "hello";
		o.BadRequestIfNull().Should().Be("hello");
	}

	[Fact]
	public void BadRequestIfNull_CustomErrorCodeAndMessage_PopulatesBothOnException()
	{
		string? o = null;
		try
		{
			o.BadRequestIfNull("CODE", "msg");
		}
		catch (DomainHttpCodeException ex)
		{
			ex.ErrorCode.Should().Be("CODE");
			ex.ErrorMessage.Should().Be("msg");
		}
	}

	[Fact]
	public void BadRequestIfNull_Struct_UnwrapsNullableToNonNullableValue()
	{
		int? v = 5;
		int unwrapped = v.BadRequestIfNull();
		unwrapped.Should().Be(5);
	}

	[Fact]
	public async Task BadRequestIfNull_TaskOverload_AwaitsAndValidates()
	{
		Task<string?> t = Task.FromResult<string?>("x");
		(await t.BadRequestIfNull()).Should().Be("x");

		Task<string?> nullT = Task.FromResult<string?>(null);
		Func<Task> act = () => nullT.BadRequestIfNull();
		await act.Should().ThrowAsync<DomainHttpCodeException>();
	}

	[Fact]
	public void UnauthorizedIfNull_WithNull_Throws401()
	{
		string? o = null;
		Action act = () => o.UnauthorizedIfNull();
		act.Should().Throw<DomainHttpCodeException>().Which.Code.Should().Be(401);
	}

	[Fact]
	public void ForbiddenIfNull_WithNull_Throws403()
	{
		string? o = null;
		Action act = () => o.ForbiddenIfNull();
		act.Should().Throw<DomainHttpCodeException>().Which.Code.Should().Be(403);
	}

	[Fact]
	public void NotFoundIfNull_WithNull_Throws404()
	{
		string? o = null;
		Action act = () => o.NotFoundIfNull();
		act.Should().Throw<DomainHttpCodeException>().Which.Code.Should().Be(404);
	}

	[Fact]
	public void DomainHttpCodeExceptionIfNull_CustomStatus_ThrowsWithCustomCode()
	{
		string? o = null;
		Action act = () => o.DomainHttpCodeExceptionIfNull(HttpStatusCode.Conflict);
		act.Should().Throw<DomainHttpCodeException>().Which.Code.Should().Be(409);
	}

	[Fact]
	public void DomainExceptionIfNull_WithNull_ThrowsDomainException()
	{
		string? o = null;
		Action act = () => o.DomainExceptionIfNull("CODE");
		DomainException ex = act.Should().Throw<DomainException>().Which;
		ex.Should().NotBeOfType<DomainHttpCodeException>();
		ex.ErrorCode.Should().Be("CODE");
	}

	[Fact]
	public void BadRequestIfTrue_WithTrue_Throws()
	{
		Action act = () => true.BadRequestIfTrue();
		act.Should().Throw<DomainHttpCodeException>().Which.Code.Should().Be(400);
	}

	[Fact]
	public void BadRequestIfTrue_WithFalse_ReturnsFalse()
	{
		false.BadRequestIfTrue().Should().BeFalse();
	}

	[Fact]
	public void BadRequestIfFalse_WithFalse_Throws()
	{
		Action act = () => false.BadRequestIfFalse();
		act.Should().Throw<DomainHttpCodeException>().Which.Code.Should().Be(400);
	}

	[Fact]
	public void BadRequestIfFalse_WithTrue_ReturnsTrue()
	{
		true.BadRequestIfFalse().Should().BeTrue();
	}

	[Fact]
	public async Task UnauthorizedIfTrue_TaskOverload_AwaitsAndThrows()
	{
		Task<bool> t = Task.FromResult(true);
		Func<Task> act = () => t.UnauthorizedIfTrue();
		(await act.Should().ThrowAsync<DomainHttpCodeException>()).Which.Code.Should().Be(401);
	}

	[Fact]
	public void ForbiddenIfFalse_WithCustomCodeAndMessage_PopulatesBoth()
	{
		try
		{
			false.ForbiddenIfFalse("C", "m");
		}
		catch (DomainHttpCodeException ex)
		{
			ex.Code.Should().Be(403);
			ex.ErrorCode.Should().Be("C");
			ex.ErrorMessage.Should().Be("m");
		}
	}

	[Fact]
	public void NotFoundIfTrue_WithFalse_ReturnsFalse()
	{
		false.NotFoundIfTrue().Should().BeFalse();
	}

	[Fact]
	public void DomainExceptionIfTrue_WithTrue_ThrowsDomainException()
	{
		Action act = () => true.DomainExceptionIfTrue("C");
		act.Should().Throw<DomainException>().Which.Should().NotBeOfType<DomainHttpCodeException>();
	}

	[Fact]
	public void DomainExceptionIfFalse_WithFalse_ThrowsDomainException()
	{
		Action act = () => false.DomainExceptionIfFalse("C");
		act.Should().Throw<DomainException>().Which.Should().NotBeOfType<DomainHttpCodeException>();
	}
}
