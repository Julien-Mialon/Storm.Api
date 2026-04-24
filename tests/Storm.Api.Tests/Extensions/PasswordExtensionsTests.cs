using Storm.Api.Extensions;

namespace Storm.Api.Tests.Extensions;

public class PasswordExtensionsTests
{
	[Fact]
	public void AsSha256_KnownInput_MatchesKnownHash()
	{
		"abc".AsSha256().Should().Be("BA7816BF8F01CFEA414140DE5DAE2223B00361A396177A9CB410FF61F20015AD");
	}

	[Fact]
	public void AsSha256_EmptyString_MatchesKnownEmptyHash()
	{
		"".AsSha256().Should().Be("E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855");
	}

	[Fact]
	public void AsSha256_SameInput_Deterministic()
	{
		"hello".AsSha256().Should().Be("hello".AsSha256());
	}

	[Fact]
	public void AsSha256_DifferentInputs_ProduceDifferentHashes()
	{
		"hello".AsSha256().Should().NotBe("world".AsSha256());
	}
}
