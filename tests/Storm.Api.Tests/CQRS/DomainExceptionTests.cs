using Storm.Api.CQRS.Exceptions;

namespace Storm.Api.Tests.CQRS;

public class DomainExceptionTests
{
	[Fact]
	public void Constructor_ErrorCodeAndMessage_SetsProperties()
	{
		DomainException ex = new("CODE", "msg");
		ex.ErrorCode.Should().Be("CODE");
		ex.ErrorMessage.Should().Be("msg");
	}

	[Fact]
	public void Constructor_WithInner_PreservesInnerExceptionChain()
	{
		Exception inner = new("inner");
		DomainException ex = new("CODE", "msg", inner);
		ex.InnerException.Should().BeSameAs(inner);
	}

	[Fact]
	public void Message_IsForwardedToBaseException()
	{
		DomainException ex = new("CODE", "forwarded");
		ex.Message.Should().Be("forwarded");
	}

	[Fact]
	public void EmptyErrorCode_IsAllowed()
	{
		DomainException ex = new("", "x");
		ex.ErrorCode.Should().Be("");
	}
}
