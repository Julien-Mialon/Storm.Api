using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Storm.Api.Extensions;

namespace Storm.Api.Tests.Extensions;

public class HttpRequestExtensionsTests
{
	private static HttpContext MakeContext(
		Dictionary<string, StringValues>? headers = null,
		Dictionary<string, StringValues>? query = null,
		IRequestCultureFeature? culture = null)
	{
		DefaultHttpContext ctx = new();
		if (headers != null)
		{
			foreach ((string k, StringValues v) in headers)
			{
				ctx.Request.Headers[k] = v;
			}
		}
		if (query != null)
		{
			ctx.Request.Query = new QueryCollection(query);
		}
		if (culture != null)
		{
			ctx.Features.Set(culture);
		}
		return ctx;
	}

	[Fact]
	public void TryGetHeader_HeaderPresent_ReturnsTrimmedValue()
	{
		HttpContext ctx = MakeContext(headers: new() { ["X-Thing"] = "  hello  " });
		ctx.Request.TryGetHeader("X-Thing", out string? v).Should().BeTrue();
		v.Should().Be("hello");
	}

	[Fact]
	public void TryGetHeader_HeaderMissing_ReturnsFalse()
	{
		HttpContext ctx = MakeContext();
		ctx.Request.TryGetHeader("X-Thing", out string? v).Should().BeFalse();
		v.Should().BeNull();
	}

	[Fact]
	public void TryGetHeader_WhitespaceOnly_TreatedAsMissing()
	{
		HttpContext ctx = MakeContext(headers: new() { ["X-Thing"] = "   " });
		ctx.Request.TryGetHeader("X-Thing", out string? v).Should().BeFalse();
		v.Should().BeNull();
	}

	[Fact]
	public void TryGetQueryParameter_Present_ReturnsTrimmedValue()
	{
		HttpContext ctx = MakeContext(query: new() { ["q"] = "  hi " });
		ctx.Request.TryGetQueryParameter("q", out string? v).Should().BeTrue();
		v.Should().Be("hi");
	}

	[Fact]
	public void TryGetQueryParameter_Missing_ReturnsFalse()
	{
		HttpContext ctx = MakeContext();
		ctx.Request.TryGetQueryParameter("q", out string? v).Should().BeFalse();
		v.Should().BeNull();
	}

	[Fact]
	public void TryGetHeaderOrQueryParameter_HeaderAndQueryBothPresent_HeaderWins()
	{
		HttpContext ctx = MakeContext(
			headers: new() { ["X-Thing"] = "from-header" },
			query: new() { ["thing"] = "from-query" });

		ctx.Request.TryGetHeaderOrQueryParameter("X-Thing", "thing", out string? v).Should().BeTrue();
		v.Should().Be("from-header");
	}

	[Fact]
	public void TryGetHeaderOrQueryParameter_OnlyQuery_ReturnsQueryValue()
	{
		HttpContext ctx = MakeContext(query: new() { ["thing"] = "from-query" });
		ctx.Request.TryGetHeaderOrQueryParameter("X-Thing", "thing", out string? v).Should().BeTrue();
		v.Should().Be("from-query");
	}

	[Fact]
	public void TryGetHeaderOrQueryParameter_Neither_ReturnsFalse()
	{
		HttpContext ctx = MakeContext();
		ctx.Request.TryGetHeaderOrQueryParameter("X-Thing", "thing", out string? v).Should().BeFalse();
		v.Should().BeNull();
	}

	[Fact]
	public void RequestCulture_FeatureMissing_ReturnsInvariantCulture()
	{
		HttpContext ctx = MakeContext();
		ctx.Request.RequestCulture().Should().Be(CultureInfo.InvariantCulture);
	}

	[Fact]
	public void RequestCulture_FeaturePresent_ReturnsResolvedCulture()
	{
		RequestCulture rc = new(new CultureInfo("fr-FR"));
		HttpContext ctx = MakeContext(culture: new RequestCultureFeature(rc, null!));
		ctx.Request.RequestCulture().Name.Should().Be("fr-FR");
	}
}
