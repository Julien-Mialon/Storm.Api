using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Storm.Api.Authentications.Jwts;

public interface IJwtRefreshCookieService
{
	bool TryReadRefreshToken([NotNullWhen(true)] out string? token);

	void SetRefreshCookie(string token, TimeSpan duration);

	void ClearRefreshCookie();

	string? GenerateCsrfToken(string refreshToken);

	bool ValidateCsrfHeader(string refreshToken);
}

public class JwtRefreshCookieService : IJwtRefreshCookieService
{
	private readonly IHttpContextAccessor _contextAccessor;
	private readonly JwtRefreshCookieConfiguration _cookieConfig;

	public JwtRefreshCookieService(IHttpContextAccessor contextAccessor, JwtRefreshCookieConfiguration cookieConfig)
	{
		_contextAccessor = contextAccessor;
		_cookieConfig = cookieConfig;
	}

	public bool TryReadRefreshToken([NotNullWhen(true)] out string? token)
	{
		HttpContext? context = _contextAccessor.HttpContext;
		if (context is null)
		{
			token = null;
			return false;
		}

		return context.Request.Cookies.TryGetValue(_cookieConfig.CookieName, out token)
			&& token is { Length: > 0 };
	}

	public void SetRefreshCookie(string token, TimeSpan duration)
	{
		HttpContext context = _contextAccessor.HttpContext
			?? throw new InvalidOperationException("No active HTTP context when setting refresh cookie.");

		context.Response.Cookies.Append(_cookieConfig.CookieName, token, new()
		{
			HttpOnly = _cookieConfig.HttpOnly,
			Secure = _cookieConfig.Secure,
			SameSite = _cookieConfig.SameSite,
			Path = _cookieConfig.CookiePath,
			Expires = DateTimeOffset.UtcNow.Add(duration),
			IsEssential = true,
		});
	}

	public void ClearRefreshCookie()
	{
		HttpContext? context = _contextAccessor.HttpContext;
		if (context is null)
		{
			return;
		}

		context.Response.Cookies.Delete(_cookieConfig.CookieName, new()
		{
			HttpOnly = _cookieConfig.HttpOnly,
			Secure = _cookieConfig.Secure,
			SameSite = _cookieConfig.SameSite,
			Path = _cookieConfig.CookiePath,
		});
	}

	/// <summary>
	/// Derives a CSRF token from the given refresh token using HMAC-SHA512.
	/// Returns null when <see cref="JwtRefreshCookieConfiguration.CsrfKey" /> is not configured.
	/// </summary>
	public string? GenerateCsrfToken(string refreshToken)
	{
		if (_cookieConfig.CsrfKey is null)
		{
			return null;
		}

		using HMACSHA512 hmac = new(_cookieConfig.CsrfKey);
		byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
		return Convert.ToBase64String(hash);
	}

	/// <summary>
	/// Validates the CSRF header against the given refresh token.
	/// Always returns true when <see cref="JwtRefreshCookieConfiguration.CsrfKey" /> is not configured.
	/// </summary>
	public bool ValidateCsrfHeader(string refreshToken)
	{
		if (_cookieConfig.CsrfKey is null)
		{
			return true;
		}

		HttpContext? context = _contextAccessor.HttpContext;
		if (context is null)
		{
			return false;
		}

		if (!context.Request.Headers.TryGetValue(_cookieConfig.CsrfHeaderName, out StringValues headerValues))
		{
			return false;
		}

		string? csrfHeader = headerValues.FirstOrDefault();
		if (csrfHeader is null)
		{
			return false;
		}

		string expected = GenerateCsrfToken(refreshToken)!;

		byte[] expectedBytes = Encoding.UTF8.GetBytes(expected);
		byte[] actualBytes = Encoding.UTF8.GetBytes(csrfHeader);

		if (expectedBytes.Length != actualBytes.Length)
		{
			return false;
		}

		return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
	}
}