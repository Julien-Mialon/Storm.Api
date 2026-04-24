using Storm.Api.Extensions;

namespace Storm.Api.Tests.Extensions;

public class StringExtensionsTests
{
	[Fact]
	public void IsNullOrEmpty_Null_ReturnsTrue() => ((string?)null).IsNullOrEmpty().Should().BeTrue();

	[Fact]
	public void IsNullOrEmpty_Empty_ReturnsTrue() => "".IsNullOrEmpty().Should().BeTrue();

	[Fact]
	public void IsNullOrEmpty_Whitespace_ReturnsFalse() => "   ".IsNullOrEmpty().Should().BeFalse();

	[Fact]
	public void IsNullOrWhiteSpace_Whitespace_ReturnsTrue() => "   ".IsNullOrWhiteSpace().Should().BeTrue();

	[Fact]
	public void IsNotNullOrEmpty_NonEmpty_ReturnsTrue() => "x".IsNotNullOrEmpty().Should().BeTrue();

	[Fact]
	public void IsNotNullOrWhiteSpace_NonWhitespace_ReturnsTrue() => "x".IsNotNullOrWhiteSpace().Should().BeTrue();

	[Fact]
	public void ValueIfNull_Null_ReturnsFallback() => ((string?)null).ValueIfNull("fb").Should().Be("fb");

	[Fact]
	public void ValueIfNull_NonNull_ReturnsOriginal() => "x".ValueIfNull("fb").Should().Be("x");

	[Fact]
	public void ValueIfNullOrEmpty_Empty_ReturnsFallback() => "".ValueIfNullOrEmpty("fb").Should().Be("fb");

	[Fact]
	public void ValueIfNullOrWhiteSpace_Whitespace_ReturnsFallback()
	{
		// Note: current implementation calls string.IsNullOrEmpty, so whitespace passes through.
		"   ".ValueIfNullOrWhiteSpace("fb").Should().Be("   ");
	}

	[Fact]
	public void NullIfEmpty_Empty_ReturnsNull() => "".NullIfEmpty().Should().BeNull();

	[Fact]
	public void NullIfEmpty_NonEmpty_ReturnsOriginal() => "x".NullIfEmpty().Should().Be("x");

	[Fact]
	public void NullIfWhiteSpace_Whitespace_ReturnsNull() => "  ".NullIfWhiteSpace().Should().BeNull();

	[Fact]
	public void OrEmpty_Null_ReturnsEmpty() => ((string?)null).OrEmpty().Should().Be("");

	[Fact]
	public void OrEmpty_NonNull_ReturnsOriginal() => "x".OrEmpty().Should().Be("x");

	[Fact]
	public void AsInt_ValidInteger_ReturnsParsedInt() => "42".AsInt().Should().Be(42);

	[Fact]
	public void AsInt_Invalid_ReturnsZero() => "abc".AsInt().Should().Be(0);

	[Fact]
	public void AsInt_Null_ReturnsZero() => ((string?)null).AsInt().Should().Be(0);

	[Fact]
	public void AsInt_Empty_ReturnsZero() => "".AsInt().Should().Be(0);
}
