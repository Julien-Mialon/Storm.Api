using System.Net;
using Storm.Api.CQRS.Exceptions;

namespace Storm.Api.Tests.CQRS;

public class DomainDatabaseExceptionTests
{
	[Fact]
	public void DefaultConstructor_ReturnsServiceUnavailable503()
	{
		new DomainDatabaseException().Code.Should().Be(503);
	}

	[Fact]
	public void Constructor_WithMessage_KeepsDefault503()
	{
		DomainDatabaseException ex = new("msg");
		ex.Code.Should().Be(503);
		ex.ErrorMessage.Should().Be("msg");
	}

	[Fact]
	public void PrimaryUnavailable_Factory_SetsPrimaryUnavailableErrorCode()
	{
		DomainDatabaseException ex = DomainDatabaseException.PrimaryUnavailable();
		ex.ErrorCode.Should().Be(DomainDatabaseException.PrimaryUnavailableErrorCode);
	}

	[Fact]
	public void PrimaryUnavailable_WithReason_UsesReasonAsErrorMessage()
	{
		DomainDatabaseException.PrimaryUnavailable("custom reason").ErrorMessage.Should().Be("custom reason");
	}

	[Fact]
	public void PrimaryUnavailable_WithoutReason_UsesDefaultMessage()
	{
		DomainDatabaseException.PrimaryUnavailable().ErrorMessage.Should().Be("Database primary replica is unavailable.");
	}

	[Fact]
	public void Constructor_CustomHttpCode_OverridesDefault503()
	{
		DomainDatabaseException ex = new(HttpStatusCode.BadGateway, "C", "m");
		ex.Code.Should().Be(502);
	}

	[Fact]
	public void Constructor_WithInner_PreservesInnerException()
	{
		Exception inner = new();
		DomainDatabaseException ex = new("C", "m", inner);
		ex.InnerException.Should().BeSameAs(inner);
	}
}
