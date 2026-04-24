using Storm.Api.CQRS.Domains.Parameters;

namespace Storm.Api.Tests.CQRS.Domains;

public class PaginationParameterTests
{
	[Fact]
	public void Defaults_PageAndCountZero()
	{
		PaginationParameter p = new();
		p.Page.Should().Be(0);
		p.Count.Should().Be(0);
	}

	[Fact]
	public void Setters_MutateValues()
	{
		PaginationParameter p = new() { Page = 3, Count = 25 };
		p.Page.Should().Be(3);
		p.Count.Should().Be(25);
	}
}
