using Storm.Api.Authentications.Jwts;
using Storm.Api.Dtos;

namespace Storm.Api.Authentications.Refresh.Handlers;

internal class CookieRefreshTokenHandler : IRefreshTokenHandler
{
	private readonly IJwtRefreshCookieService _cookieService;

	public CookieRefreshTokenHandler(IJwtRefreshCookieService cookieService)
	{
		_cookieService = cookieService;
	}

	public string? ReadInboundToken(RefreshTokenParameter parameter)
	{
		return _cookieService.TryReadRefreshToken(out string? token) ? token : null;
	}

	public bool ValidateTransport(string refreshToken)
	{
		return _cookieService.ValidateCsrfHeader(refreshToken);
	}

	public Task<bool> ValidateTokenStateAsync(string refreshToken, string jti)
	{
		return Task.FromResult(true);
	}

	public Task StoreAndEmitAsync(Guid accountId, string refreshToken, string jti,
		TimeSpan duration, LoginResponse response)
	{
		_cookieService.SetRefreshCookie(refreshToken, duration);
		response.CsrfToken = _cookieService.GenerateCsrfToken(refreshToken);
		return Task.CompletedTask;
	}

	public Task RevokeAsync(string? refreshToken, string? jti)
	{
		_cookieService.ClearRefreshCookie();
		return Task.CompletedTask;
	}
}