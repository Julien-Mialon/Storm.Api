namespace Storm.Api.Authentications.Refresh.Storage;

internal class JwtRefreshTokenStorage : IRefreshTokenStorage
{
	public Task StoreAsync(Guid accountId, string jti, DateTime expiresAt)
	{
		return Task.CompletedTask;
	}

	public Task<bool> ValidateAsync(string jti)
	{
		return Task.FromResult(true);
	}

	public Task<bool> RotateAsync(string oldJti, Guid accountId, string newJti, DateTime newExpiresAt)
	{
		return Task.FromResult(true);
	}

	public Task RevokeAsync(string jti)
	{
		return Task.CompletedTask;
	}

	public Task RevokeAllForAccountAsync(Guid accountId)
	{
		return Task.CompletedTask;
	}
}