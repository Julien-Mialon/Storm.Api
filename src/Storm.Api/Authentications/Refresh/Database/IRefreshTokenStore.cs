namespace Storm.Api.Authentications.Refresh.Database;

public interface IRefreshTokenStore
{
	Task StoreAsync(Guid accountId, string jti, DateTime expiresAt);

	Task<bool> ExistsAsync(string jti);

	Task RevokeAsync(string jti);

	Task RevokeAllForAccountAsync(Guid accountId);

	Task CleanupExpiredAsync();
}