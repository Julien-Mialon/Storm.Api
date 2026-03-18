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
			DateTime now = Resolve<TimeProvider>().GetUtcNow().UtcDateTime;
			await connection.InsertAsync(new RefreshTokenEntity
			{
				Id = Guid.NewGuid(),
				AccountId = accountId,
				Jti = jti,
				ExpiresAt = expiresAt,
			});
		});
	}

	public async Task<bool> ExistsAsync(string jti)
	{
		return await UseReadConnection(async connection =>
		{
			DateTime now = Resolve<TimeProvider>().GetUtcNow().UtcDateTime;
			return await connection.From<RefreshTokenEntity>()
				.Where(x => x.Jti == jti && x.ExpiresAt > now)
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
			DateTime now = Resolve<TimeProvider>().GetUtcNow().UtcDateTime;
			await connection.DeleteAsync<RefreshTokenEntity>(x => x.ExpiresAt <= now);
		});
	}
}