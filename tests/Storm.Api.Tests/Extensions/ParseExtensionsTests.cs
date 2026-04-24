using Storm.Api.Extensions;

namespace Storm.Api.Tests.Extensions;

public class ParseExtensionsTests
{
	[Fact]
	public void ToGuid_ValidString_ReturnsGuid()
	{
		Guid guid = Guid.NewGuid();
		guid.ToString().ToGuid().Should().Be(guid);
	}

	[Fact]
	public void ToGuid_InvalidString_ReturnsNull()
	{
		"not-a-guid".ToGuid().Should().BeNull();
	}

	[Fact]
	public void ToGuid_EmptyOrWhitespace_ReturnsNull()
	{
		"".ToGuid().Should().BeNull();
		"   ".ToGuid().Should().BeNull();
	}
}
