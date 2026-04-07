namespace Storm.Api.Authentications.Refresh.Storage;

public interface IRefreshTokenStorage
{
	Task StoreAsync(Guid accountId, string jti, DateTime expiresAt);

	/// <summary>
	/// Checks whether the JTI exists and is not expired (read-only, no side effects).
	/// Used for early rejection before expensive operations.
	/// </summary>
	Task<bool> ValidateAsync(string jti);

	/// <summary>
	/// Atomically validates that the old JTI exists and is not expired, revokes it,
	/// and stores the new JTI.
	/// Returns true if the old token was valid and rotation succeeded; false if the
	/// old token was already revoked or expired (prevents concurrent replay).
	/// Must be called within a transaction opened by the caller.
	/// </summary>
	Task<bool> RotateAsync(string oldJti, Guid accountId, string newJti, DateTime newExpiresAt);

	Task RevokeAsync(string jti);

	Task RevokeAllForAccountAsync(Guid accountId);
}