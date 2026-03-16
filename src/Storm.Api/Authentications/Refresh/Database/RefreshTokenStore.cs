using ServiceStack.OrmLite;
using Storm.Api.Databases.Extensions;
using Storm.Api.Databases.Services;

namespace Storm.Api.Authentications.Refresh.Database;

internal class RefreshTokenStore : BaseDatabaseService, IRefreshTokenStore
{
	public RefreshTokenStore(IServiceProvider services) : base(services)
	{
	}

	public async Task StoreAsync(Guid accountId, string jti, DateTime expiresAt)
	{
		await UseWriteConnection(async connection =>
		{
			await connection.InsertAsync(new RefreshTokenEntity
			{
				Id = Guid.NewGuid(),
				AccountId = accountId,
				Jti = jti,
				ExpiresAt = expiresAt,
				EntityCreatedDate = DateTime.UtcNow,
			});
		});
	}

	public async Task<bool> ExistsAsync(string jti)
	{
		return await UseReadConnection(async connection =>
		{
			return await connection.From<RefreshTokenEntity>()
				.Where(x => x.Jti == jti && x.ExpiresAt > DateTime.UtcNow)
				.AsExistsAsync(connection);
		});
	}

	public async Task RevokeAsync(string jti)
	{
		await UseWriteConnection(async connection =>
		{
			await connection.DeleteAsync<RefreshTokenEntity>(x => x.Jti == jti);
		});
	}

	public async Task RevokeAllForAccountAsync(Guid accountId)
	{
		await UseWriteConnection(async connection =>
		{
			await connection.DeleteAsync<RefreshTokenEntity>(x => x.AccountId == accountId);
		});
	}

	public async Task CleanupExpiredAsync()
	{
		await UseWriteConnection(async connection =>
		{
			await connection.DeleteAsync<RefreshTokenEntity>(x => x.ExpiresAt <= DateTime.UtcNow);
		});
	}
}