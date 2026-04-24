using Storm.Api.Extensions;

namespace Storm.Api.Tests.Extensions;

public class ByteExtensionsTests
{
	[Fact]
	public void ToHexString_EmptyArray_ReturnsEmptyString()
	{
		Array.Empty<byte>().ToHexString().Should().Be(string.Empty);
	}

	[Fact]
	public void ToHexString_AllZeroBytes_ReturnsZeros()
	{
		new byte[] { 0, 0, 0 }.ToHexString().Should().Be("000000");
	}

	[Fact]
	public void ToHexString_AllFFBytes_ReturnsFFs()
	{
		new byte[] { 0xFF, 0xFF }.ToHexString().Should().Be("FFFF");
	}

	[Fact]
	public void ToHexString_MixedBytes_ReturnsCorrectHex()
	{
		new byte[] { 0x01, 0x23, 0xAB, 0xCD }.ToHexString().Should().Be("0123ABCD");
	}

	[Fact]
	public void ToHexString_OutputIsUppercase()
	{
		string result = new byte[] { 0xAB, 0xCD, 0xEF }.ToHexString();
		result.Should().Be("ABCDEF");
		result.Should().MatchRegex("^[0-9A-F]+$");
	}
}
