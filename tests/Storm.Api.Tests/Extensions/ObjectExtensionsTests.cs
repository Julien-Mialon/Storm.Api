using Storm.Api.Extensions;

namespace Storm.Api.Tests.Extensions;

public class ObjectExtensionsTests
{
	[Fact]
	public void IsNull_Null_ReturnsTrue() => ((object?)null).IsNull().Should().BeTrue();

	[Fact]
	public void IsNull_NonNull_ReturnsFalse() => new object().IsNull().Should().BeFalse();

	[Fact]
	public void IsNotNull_Null_ReturnsFalse() => ((object?)null).IsNotNull().Should().BeFalse();

	[Fact]
	public void IsNotNull_NonNull_ReturnsTrue() => new object().IsNotNull().Should().BeTrue();
}
