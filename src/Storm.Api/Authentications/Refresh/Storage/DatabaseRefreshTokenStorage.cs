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
			await connection.InsertAsync(new RefreshTokenEntity
			{
				Id = Guid.NewGuid(),
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
			return await connection.From<RefreshTokenEntity>()
				.Where(x => x.Jti == jti && x.ExpiresAt > DateTime.UtcNow)
				.AsExistsAsync(connection);
		});
	}

	public async Task<bool> RotateAsync(string oldJti, Guid accountId, string newJti, DateTime newExpiresAt)
	{
		return await UseWriteConnection(async connection =>
		{
			int deleted = await connection.DeleteAsync<RefreshTokenEntity>(x => x.Jti == oldJti && x.ExpiresAt > DateTime.UtcNow);
			if (deleted == 0)
			{
				return false;
			}

			await connection.InsertAsync(new RefreshTokenEntity
			{
				Id = Guid.NewGuid(),
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