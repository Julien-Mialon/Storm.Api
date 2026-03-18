using Storm.Api.Authentications.Refresh.Database;
using Storm.Api.Dtos;

namespace Storm.Api.Authentications.Refresh.Handlers;

internal class DatabaseRefreshTokenHandler : IRefreshTokenHandler
{
	private readonly IRefreshTokenStore _store;
	private readonly TimeProvider _timeProvider;

	public DatabaseRefreshTokenHandler(IRefreshTokenStore store, TimeProvider timeProvider)
	{
		_store = store;
		_timeProvider = timeProvider;
	}

	public string? ReadInboundToken(RefreshTokenParameter parameter)
	{
		return string.IsNullOrEmpty(parameter.RefreshToken) ? null : parameter.RefreshToken;
	}

	public bool ValidateTransport(string refreshToken)
	{
		return true;
	}

	public Task<bool> ValidateTokenStateAsync(string refreshToken, string jti)
	{
		return _store.ExistsAsync(jti);
	}

	public async Task StoreAndEmitAsync(Guid accountId, string refreshToken, string jti, TimeSpan duration, LoginResponse response)
	{
		await _store.StoreAsync(accountId, jti, _timeProvider.GetUtcNow().UtcDateTime.Add(duration));
		response.RefreshToken = refreshToken;
	}

	public Task RevokeAsync(string? refreshToken, string? jti)
	{
		if (jti is not null)
		{
			return _store.RevokeAsync(jti);
		}

		return Task.CompletedTask;
	}
}