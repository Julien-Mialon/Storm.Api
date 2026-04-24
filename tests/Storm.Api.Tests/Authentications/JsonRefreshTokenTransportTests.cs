using Storm.Api.Authentications.Refresh;
using Storm.Api.Authentications.Refresh.Transport;
using Storm.Api.Dtos;

namespace Storm.Api.Tests.Authentications;

public class JsonRefreshTokenTransportTests
{
	private static IRefreshTokenTransport NewTransport()
	{
		Type t = typeof(IRefreshTokenTransport).Assembly.GetType("Storm.Api.Authentications.Refresh.Transport.JsonRefreshTokenTransport")!;
		return (IRefreshTokenTransport)Activator.CreateInstance(t)!;
	}

	[Fact]
	public void ReadToken_JsonBodyContainsToken_ReturnsValue()
	{
		IRefreshTokenTransport t = NewTransport();
		RefreshTokenParameter p = new() { RefreshToken = "rt" };
		t.ReadToken(p).Should().Be("rt");
	}

	[Fact]
	public void ReadToken_JsonBodyEmpty_ReturnsNull()
	{
		IRefreshTokenTransport t = NewTransport();
		RefreshTokenParameter p = new() { RefreshToken = "" };
		t.ReadToken(p).Should().BeNull();
	}

	[Fact]
	public void WriteToken_ReturnsJsonResponseWithToken()
	{
		IRefreshTokenTransport t = NewTransport();
		LoginResponse r = new();
		t.EmitToken("newrt", TimeSpan.FromMinutes(5), r);
		r.RefreshToken.Should().Be("newrt");
	}

	[Fact]
	public void ValidateTransport_AlwaysTrue()
	{
		NewTransport().ValidateTransport("anything").Should().BeTrue();
	}
}
