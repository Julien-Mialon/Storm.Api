using ServiceStack.OrmLite;
using Storm.Api.Databases.Extensions;
using Storm.Api.Databases.Services;

namespace Storm.Api.Authentications.Refresh.Storage;

internal class DatabaseRefreshTokenStorage : BaseDatabaseService, IRefreshTokenStorage
{
	public DatabaseRefreshTokenStorage(IServiceProvider services) : base(services)
	{
	}

	public async Task StoreAsync(Guid accountId, string jti, DateTime expiresAt)
	{
		await UseWriteConnection(async connection =>
		{
			DateTime now = Resolve<TimeProvider>().GetUtcNow().UtcDateTime;
			await connection.InsertAsync(new RefreshTokenEntity
			{
				AccountId = accountId,
				Jti = jti,
				ExpiresAt = expiresAt,
			});
		});
	}

	public async Task<bool> ValidateAsync(string jti)
	{
		return await UseReadConnection(async connection =>
		{
			DateTime now = Resolve<TimeProvider>().GetUtcNow().UtcDateTime;
			return await connection.From<RefreshTokenEntity>()
				.Where(x => x.Jti == jti && x.ExpiresAt > now)
				.AsExistsAsync(connection);
		});
	}

	public async Task<bool> RotateAsync(string oldJti, Guid accountId, string newJti, DateTime newExpiresAt)
	{
		return await UseWriteConnection(async connection =>
		{
			DateTime now = Resolve<TimeProvider>().GetUtcNow().UtcDateTime;
			int deleted = await connection.DeleteAsync<RefreshTokenEntity>(x => x.Jti == oldJti && x.ExpiresAt > now);
			if (deleted == 0)
			{
				return false;
			}

			await connection.InsertAsync(new RefreshTokenEntity
			{
				AccountId = accountId,
				Jti = newJti,
				ExpiresAt = newExpiresAt,
			});

			return true;
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
}