using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Storm.Api.Authentications.Refresh.Transport;

public class CookieTransportConfiguration
{
	public required string CookieName { get; init; }

	public required string CookiePath { get; init; }

	public bool Secure { get; init; } = true;

	public bool HttpOnly { get; init; } = true;

	public SameSiteMode SameSite { get; init; } = SameSiteMode.Strict;

	/// <summary>
	/// Optional cookie domain (e.g. ".example.com" for cross-subdomain sharing).
	/// When null, the cookie is scoped to the current host only.
	/// </summary>
	public string? Domain { get; init; }

	/// <summary>
	/// When set, CSRF validation is enabled on the refresh endpoint.
	/// The CSRF token is derived as HMAC-SHA512(CsrfKey, refreshToken) and must be
	/// sent by the client as the <see cref="CsrfHeaderName" /> request header.
	/// Required when SameSite is not Strict.
	/// </summary>
	public byte[]? CsrfKey { get; init; }

	public string CsrfHeaderName { get; init; } = "X-CSRF-Token";
}

public static class CookieTransportConfigurationLoader
{
	public static CookieTransportConfiguration LoadCookieTransportConfiguration(this IConfiguration configuration)
	{
		SameSiteMode sameSite = SameSiteMode.Strict;
		string? sameSiteValue = configuration.GetValue<string>("SameSite");
		if (sameSiteValue is not null && Enum.TryParse(sameSiteValue, true, out SameSiteMode parsed))
		{
			sameSite = parsed;
		}

		string? csrfKeyValue = configuration.GetValue<string>("CsrfKey");
		byte[]? csrfKey = csrfKeyValue is not null ? Convert.FromBase64String(csrfKeyValue) : null;

		if (sameSite is not SameSiteMode.Strict && csrfKey is null)
		{
			throw new InvalidOperationException($"CsrfKey is required in the cookie transport configuration when SameSite is {sameSite}. "
				+ "Set a base64-encoded CsrfKey, or use SameSite=Strict.");
		}

		return new()
		{
			CookieName = configuration.GetValue<string>("CookieName") ?? "refresh_token",
			CookiePath = configuration.GetValue<string>("CookiePath") ?? "/auth/refresh",
			Secure = configuration.GetValue<bool?>("Secure") ?? true,
			HttpOnly = configuration.GetValue<bool?>("HttpOnly") ?? true,
			SameSite = sameSite,
			Domain = configuration.GetValue<string>("Domain"),
			CsrfKey = csrfKey,
			CsrfHeaderName = configuration.GetValue<string>("CsrfHeaderName") ?? "X-CSRF-Token",
		};
	}
}