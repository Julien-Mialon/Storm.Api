using Storm.Api.Extensions;

namespace Storm.Api.Tests.Extensions;

public class RandomExtensionsTests
{
	[Fact]
	public void RandomAlphaString_LengthZero_ReturnsEmptyString()
	{
		0.RandomAlphaString().Should().Be("");
	}

	[Fact]
	public void RandomAlphaString_Length_ReturnsRequestedLength()
	{
		10.RandomAlphaString().Should().HaveLength(10);
	}

	[Fact]
	public void RandomAlphaString_ContainsOnlyAlphaChars()
	{
		100.RandomAlphaString().Should().MatchRegex("^[a-zA-Z]+$");
	}

	[Fact]
	public void RandomAlphaDigitsString_ContainsOnlyAlphanumericChars()
	{
		100.RandomAlphaDigitsString().Should().MatchRegex("^[a-zA-Z0-9]+$");
	}

	[Fact]
	public void RandomString_CustomCharset_ContainsOnlyThoseChars()
	{
		char[] charset = ['a', 'b', 'c'];
		string result = 100.RandomString(charset);
		result.All(c => charset.Contains(c)).Should().BeTrue();
	}

	[Fact]
	public void RandomInt_MaxExclusive_NeverEqualsMax()
	{
		for (int i = 0; i < 1000; i++)
		{
			5.Random().Should().BeInRange(0, 4);
		}
	}

	[Fact]
	public void RandomLong_MaxExclusive_NeverEqualsMax()
	{
		for (int i = 0; i < 1000; i++)
		{
			5L.Random().Should().BeInRange(0, 4);
		}
	}

	[Fact]
	public void RandomItem_SingleItemList_ReturnsThatItem()
	{
		new List<int> { 42 }.RandomItem().Should().Be(42);
	}

	[Fact]
	public void RandomItem_EmptyList_Throws()
	{
		Action act = () => new List<int>().RandomItem();
		act.Should().Throw<ArgumentOutOfRangeException>();
	}
}
