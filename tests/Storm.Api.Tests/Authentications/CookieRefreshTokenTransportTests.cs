using Microsoft.AspNetCore.Http;
using Storm.Api.Authentications.Refresh;
using Storm.Api.Authentications.Refresh.Transport;
using Storm.Api.Dtos;

namespace Storm.Api.Tests.Authentications;

public class CookieRefreshTokenTransportTests
{
	private static readonly Type TransportType = typeof(IRefreshTokenTransport).Assembly
		.GetType("Storm.Api.Authentications.Refresh.Transport.CookieRefreshTokenTransport")!;

	private sealed class Accessor(HttpContext? ctx) : IHttpContextAccessor
	{
		public HttpContext? HttpContext { get => ctx; set => ctx = value; }
	}

	private static IRefreshTokenTransport Build(HttpContext? ctx, CookieTransportConfiguration cfg)
	{
		return (IRefreshTokenTransport)Activator.CreateInstance(TransportType, new Accessor(ctx), cfg, TimeProvider.System)!;
	}

	private static CookieTransportConfiguration DefaultConfig(byte[]? csrfKey = null) => new()
	{
		CookieName = "refresh_token",
		CookiePath = "/auth/refresh",
		CsrfKey = csrfKey,
	};

	[Fact]
	public void ReadToken_CookieMissing_ReturnsNull()
	{
		DefaultHttpContext ctx = new();
		IRefreshTokenTransport t = Build(ctx, DefaultConfig());
		t.ReadToken(new RefreshTokenParameter { RefreshToken = "" }).Should().BeNull();
	}

	[Fact]
	public void ReadToken_CookiePresent_ReturnsValue()
	{
		DefaultHttpContext ctx = new();
		ctx.Request.Headers["Cookie"] = "refresh_token=abc";
		IRefreshTokenTransport t = Build(ctx, DefaultConfig());
		t.ReadToken(new RefreshTokenParameter { RefreshToken = "" }).Should().Be("abc");
	}

	[Fact]
	public void ValidateRequest_CsrfHeaderMissing_ReturnsFalse()
	{
		DefaultHttpContext ctx = new();
		byte[] key = System.Text.Encoding.UTF8.GetBytes("secret");
		IRefreshTokenTransport t = Build(ctx, DefaultConfig(key));
		t.ValidateTransport("refresh").Should().BeFalse();
	}

	[Fact]
	public void ValidateRequest_CsrfHeaderMatches_Passes()
	{
		byte[] key = System.Text.Encoding.UTF8.GetBytes("secret");
		using System.Security.Cryptography.HMACSHA512 hmac = new(key);
		string csrf = Convert.ToBase64String(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("refresh")));

		DefaultHttpContext ctx = new();
		ctx.Request.Headers["X-CSRF-Token"] = csrf;
		IRefreshTokenTransport t = Build(ctx, DefaultConfig(key));

		t.ValidateTransport("refresh").Should().BeTrue();
	}

	[Fact]
	public void WriteToken_SetsCookieWithExpectedAttributes()
	{
		DefaultHttpContext ctx = new();
		IRefreshTokenTransport t = Build(ctx, DefaultConfig());
		LoginResponse r = new();
		t.EmitToken("newrt", TimeSpan.FromMinutes(5), r);
		ctx.Response.Headers["Set-Cookie"].ToString().Should().Contain("refresh_token=newrt");
	}
}
