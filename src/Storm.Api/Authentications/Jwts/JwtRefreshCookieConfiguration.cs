using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Storm.Api.Authentications.Jwts;

public class JwtRefreshCookieConfiguration
{
	public required string CookieName { get; init; }

	public required string CookiePath { get; init; }

	public bool Secure { get; init; } = true;

	public bool HttpOnly { get; init; } = true;

	public SameSiteMode SameSite { get; init; } = SameSiteMode.Strict;

	/// <summary>
	/// When set, CSRF validation is enabled on the refresh endpoint.
	/// The CSRF token is derived as HMAC-SHA512(CsrfKey, refreshToken) and must be
	/// sent by the client as the <see cref="CsrfHeaderName" /> request header.
	/// Required when SameSite is None or Unspecified (cross-subdomain SPAs).
	/// </summary>
	public byte[]? CsrfKey { get; init; }

	public string CsrfHeaderName { get; init; } = "X-CSRF-Token";
}

public static class JwtRefreshCookieConfigurationLoader
{
	public static JwtRefreshCookieConfiguration LoadJwtRefreshCookieConfiguration(this IConfiguration configuration)
	{
		SameSiteMode sameSite = SameSiteMode.Strict;
		string? sameSiteValue = configuration.GetValue<string>("SameSite");
		if (sameSiteValue is not null && Enum.TryParse(sameSiteValue, true, out SameSiteMode parsed))
		{
			sameSite = parsed;
		}

		string? csrfKeyValue = configuration.GetValue<string>("CsrfKey");
		byte[]? csrfKey = csrfKeyValue is not null ? Convert.FromBase64String(csrfKeyValue) : null;

		// SameSite=Strict and SameSite=Lax both block cross-site POST requests, so CSRF is not needed.
		// SameSite=None sends the cookie on all cross-site requests.
		// SameSite=Unspecified emits no attribute — browser behaviour varies, treat as unsafe.
		if (sameSite is not SameSiteMode.Strict and not SameSiteMode.Lax && csrfKey is null)
		{
			throw new InvalidOperationException($"CsrfKey is required in the refresh cookie configuration when SameSite is {sameSite}. " + "Set a base64-encoded CsrfKey, or use SameSite=Strict or SameSite=Lax.");
		}

		return new()
		{
			CookieName = configuration.GetValue<string>("CookieName") ?? "refresh_token",
			CookiePath = configuration.GetValue<string>("CookiePath") ?? "/auth/refresh",
			Secure = configuration.GetValue<bool?>("Secure") ?? true,
			HttpOnly = configuration.GetValue<bool?>("HttpOnly") ?? true,
			SameSite = sameSite,
			CsrfKey = csrfKey,
			CsrfHeaderName = configuration.GetValue<string>("CsrfHeaderName") ?? "X-CSRF-Token",
		};
	}
}