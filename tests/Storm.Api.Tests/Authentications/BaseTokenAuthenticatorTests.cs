using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Authentications.Commons;

namespace Storm.Api.Tests.Authentications;

public class BaseTokenAuthenticatorTests
{
	private sealed class TestAuthenticator(IServiceProvider services, string? headerName, string? queryName, string? tokenType)
		: BaseTokenAuthenticator<string>(services, headerName, queryName, tokenType)
	{
		public string? LastTokenReceived { get; private set; }
		protected override Task<string?> Authenticate(string token)
		{
			LastTokenReceived = token;
			return Task.FromResult<string?>("account-for-" + token);
		}
	}

	private static IHttpContextAccessor Accessor(HttpContext? ctx)
		=> new HttpContextAccessor { HttpContext = ctx };

	private static IServiceProvider Provider(HttpContext? ctx)
	{
		ServiceCollection sc = new();
		sc.AddSingleton<IHttpContextAccessor>(Accessor(ctx));
		return sc.BuildServiceProvider();
	}

	[Fact]
	public async Task Authenticate_HeaderPresent_ExtractsToken()
	{
		DefaultHttpContext ctx = new();
		ctx.Request.Headers["Authorization"] = "abc";
		TestAuthenticator a = new(Provider(ctx), "Authorization", null, null);
		string? account = await a.Authenticate();
		account.Should().Be("account-for-abc");
		a.LastTokenReceived.Should().Be("abc");
	}

	[Fact]
	public async Task Authenticate_QueryPresent_ExtractsToken()
	{
		DefaultHttpContext ctx = new();
		ctx.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { ["t"] = "tok" });
		TestAuthenticator a = new(Provider(ctx), null, "t", null);
		(await a.Authenticate()).Should().Be("account-for-tok");
	}

	[Fact]
	public async Task Authenticate_BearerPrefixStripped()
	{
		DefaultHttpContext ctx = new();
		ctx.Request.Headers["Authorization"] = "Bearer abc";
		TestAuthenticator a = new(Provider(ctx), "Authorization", null, "Bearer");
		(await a.Authenticate()).Should().Be("account-for-abc");
		a.LastTokenReceived.Should().Be("abc");
	}

	[Fact]
	public async Task Authenticate_NoToken_ReturnsNull()
	{
		DefaultHttpContext ctx = new();
		TestAuthenticator a = new(Provider(ctx), "Authorization", "t", null);
		(await a.Authenticate()).Should().BeNull();
	}

	[Fact]
	public async Task Authenticate_CallsAbstractAuthenticateOnlyWithTokenString()
	{
		DefaultHttpContext ctx = new();
		ctx.Request.Headers["Authorization"] = "tok-xyz";
		TestAuthenticator a = new(Provider(ctx), "Authorization", null, null);
		await a.Authenticate();
		a.LastTokenReceived.Should().Be("tok-xyz");
	}
}
