using System.Reflection;

namespace Storm.Api.Tests.CQRS;

public class UnitTests
{
	[Fact]
	public void Default_IsNotNull()
	{
		Unit.Default.Should().NotBeNull();
	}

	[Fact]
	public void Default_ReturnsSameInstanceAcrossCalls()
	{
		Unit a = Unit.Default;
		Unit b = Unit.Default;
		a.Should().BeSameAs(b);
	}

	[Fact]
	public void Constructor_IsPrivate_PreventsDirectInstantiation()
	{
		ConstructorInfo[] publicCtors = typeof(Unit).GetConstructors(BindingFlags.Public | BindingFlags.Instance);
		publicCtors.Should().BeEmpty();

		ConstructorInfo[] privateCtors = typeof(Unit).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
		privateCtors.Should().ContainSingle().Which.IsPrivate.Should().BeTrue();
	}
}
