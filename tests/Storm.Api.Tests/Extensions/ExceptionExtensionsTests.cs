using Storm.Api.Extensions;

namespace Storm.Api.Tests.Extensions;

public class ExceptionExtensionsTests
{
	[Fact]
	public void IsDatabaseException_InvalidOperationWithDbMessage_ReturnsTrue()
	{
		new InvalidOperationException("The connection's current state is closed.").IsDatabaseException().Should().BeTrue();
		new InvalidOperationException("The connection is closed.").IsDatabaseException().Should().BeTrue();
	}

	[Fact]
	public void IsDatabaseException_InvalidOperationGeneric_ReturnsFalse()
	{
		new InvalidOperationException("unrelated").IsDatabaseException().Should().BeFalse();
	}

	[Fact]
	public void IsDatabaseException_AggregateContainingDbException_ReturnsTrue()
	{
		AggregateException ex = new(new InvalidOperationException("The connection is closed."));
		ex.IsDatabaseException().Should().BeTrue();
	}

	[Fact]
	public void IsDatabaseException_UnrelatedException_ReturnsFalse()
	{
		new Exception("boom").IsDatabaseException().Should().BeFalse();
	}

	[Fact]
	public void IsDatabaseException_SqlException_ReturnsTrue()
	{
		InvalidOperationException ex = new("The connection is closed.");
		ex.IsDatabaseException().Should().BeTrue();
	}

	[Fact]
	public void IsDatabaseException_NullRefWithOrmLiteStackTrace_ReturnsFalse_WhenNoOrmLiteStack()
	{
		new NullReferenceException().IsDatabaseException().Should().BeFalse();
	}
}
