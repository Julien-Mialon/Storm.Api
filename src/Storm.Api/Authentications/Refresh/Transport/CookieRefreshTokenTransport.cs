using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Storm.Api.Dtos;

namespace Storm.Api.Authentications.Refresh.Transport;

internal class CookieRefreshTokenTransport : IRefreshTokenTransport
{
	private readonly IHttpContextAccessor _contextAccessor;
	private readonly CookieTransportConfiguration _config;
	private readonly TimeProvider _timeProvider;

	public CookieRefreshTokenTransport(IHttpContextAccessor contextAccessor, CookieTransportConfiguration config, TimeProvider timeProvider)
	{
		_contextAccessor = contextAccessor;
		_config = config;
		_timeProvider = timeProvider;
	}

	public string? ReadToken(RefreshTokenParameter parameter)
	{
		HttpContext? context = _contextAccessor.HttpContext;
		if (context is null)
		{
			return null;
		}

		return context.Request.Cookies.TryGetValue(_config.CookieName, out string? token) && token is { Length: > 0 }
			? token
			: null;
	}

	public bool ValidateTransport(string refreshToken)
	{
		if (_config.CsrfKey is null)
		{
			return true;
		}

		HttpContext? context = _contextAccessor.HttpContext;
		if (context is null)
		{
			return false;
		}

		if (!context.Request.Headers.TryGetValue(_config.CsrfHeaderName, out StringValues headerValues))
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

	public void EmitToken(string refreshToken, TimeSpan duration, LoginResponse response)
	{
		HttpContext context = _contextAccessor.HttpContext
			?? throw new InvalidOperationException("No active HTTP context when setting refresh cookie.");

		context.Response.Cookies.Append(_config.CookieName, refreshToken, new()
		{
			HttpOnly = _config.HttpOnly,
			Secure = _config.Secure,
			SameSite = _config.SameSite,
			Path = _config.CookiePath,
			Domain = _config.Domain,
			Expires = _timeProvider.GetUtcNow().Add(duration),
			IsEssential = true,
		});

		response.CsrfToken = GenerateCsrfToken(refreshToken);
	}

	public void ClearToken()
	{
		HttpContext? context = _contextAccessor.HttpContext;
		if (context is null)
		{
			return;
		}

		context.Response.Cookies.Delete(_config.CookieName, new()
		{
			HttpOnly = _config.HttpOnly,
			Secure = _config.Secure,
			SameSite = _config.SameSite,
			Path = _config.CookiePath,
			Domain = _config.Domain,
		});
	}

	private string? GenerateCsrfToken(string refreshToken)
	{
		if (_config.CsrfKey is null)
		{
			return null;
		}

		using HMACSHA512 hmac = new(_config.CsrfKey);
		byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
		return Convert.ToBase64String(hash);
	}
}