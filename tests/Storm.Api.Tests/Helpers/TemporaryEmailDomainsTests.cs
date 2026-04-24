using Storm.Api.Helpers;

namespace Storm.Api.Tests.Helpers;

public class TemporaryEmailDomainsTests
{
	[Fact]
	public void IsTemporaryEmailDomain_KnownDisposableDomain_ReturnsTrue()
	{
		"user@10minutemail.com".IsTemporaryEmailDomain().Should().BeTrue();
	}

	[Fact]
	public void IsTemporaryEmailDomain_LegitimateDomain_ReturnsFalse()
	{
		"user@gmail.com".IsTemporaryEmailDomain().Should().BeFalse();
	}

	[Fact]
	public void IsTemporaryEmailDomain_MixedCaseDomain_MatchesCaseInsensitively()
	{
		"user@10MinuteMail.com".IsTemporaryEmailDomain().Should().BeTrue();
	}

	[Fact]
	public void IsTemporaryEmailDomain_MalformedEmail_ReturnsTrue()
	{
		// No '@' → treated as disposable (rejected).
		"notanemail".IsTemporaryEmailDomain().Should().BeTrue();
	}

	[Fact]
	public void IsTemporaryEmailDomain_Empty_ReturnsTrue()
	{
		"".IsTemporaryEmailDomain().Should().BeTrue();
	}
}
