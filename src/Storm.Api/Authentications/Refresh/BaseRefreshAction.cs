using System.Net;
using Storm.Api.Authentications.Jwts;
using Storm.Api.Authentications.Refresh.Storage;
using Storm.Api.Authentications.Refresh.Transport;
using Storm.Api.CQRS;
using Storm.Api.CQRS.Exceptions;
using Storm.Api.CQRS.Extensions;
using Storm.Api.Databases.Models;
using Storm.Api.Databases.Repositories;
using Storm.Api.Dtos;

namespace Storm.Api.Authentications.Refresh;

public abstract class BaseRefreshAction<TAccount>(IServiceProvider services)
	: BaseAction<RefreshTokenParameter, LoginResponse>(services)
	where TAccount : IGuidEntity
{
	protected sealed override async Task<LoginResponse> Action(RefreshTokenParameter parameter)
	{
		IRefreshTokenStorage storage = Resolve<IRefreshTokenStorage>();
		IRefreshTokenTransport transport = Resolve<IRefreshTokenTransportResolver>().Resolve();
		JwtService<RefreshTokenMarker> refreshSvc = Resolve<JwtService<RefreshTokenMarker>>();

		// 1. Read inbound token via transport
		string inboundToken = transport.ReadToken(parameter).UnauthorizedIfNull();

		// 2. Validate transport-level security (CSRF for cookies)
		transport.ValidateTransport(inboundToken).UnauthorizedIfFalse();

		// 3. Validate JWT signature and extract account ID
		if (refreshSvc.TryValidateToken(inboundToken, out Guid accountId) is false)
		{
			transport.ClearToken();
			throw new DomainHttpCodeException(HttpStatusCode.Unauthorized);
		}

		// 4. Extract JTI
		string inboundJti = JtiExtractor.Extract(inboundToken).UnauthorizedIfNull();

		// 5. Early validation (cheap read-only check before expensive work)
		await storage.ValidateAsync(inboundJti).UnauthorizedIfFalse();

		// 6. Load and validate account
		TAccount? account = await Resolve<IGuidRepository<TAccount>>().GetById(accountId);
		if (account is null)
		{
			transport.ClearToken();
			throw new DomainHttpCodeException(HttpStatusCode.Unauthorized);
		}

		await ValidateAccount(account);

		// 7. Generate new tokens
		DateTime utcNow = Resolve<TimeProvider>().GetUtcNow().UtcDateTime;

		JwtService<TAccount> accessSvc = Resolve<JwtService<TAccount>>();
		(string newAccessToken, TimeSpan accessDuration) = accessSvc.GenerateToken(account.Id);

		string newJti = Guid.NewGuid().ToString("N");
		(string newRefreshToken, TimeSpan refreshDuration) = refreshSvc.GenerateToken(account.Id, new Dictionary<string, string>
		{
			["jti"] = newJti,
		});

		// 8. Atomically validate+revoke old token and store new one in a transaction
		//    If ValidateAccount or any step above threw, the old token remains untouched.
		return await WithDatabaseTransaction(async () =>
		{
			await storage.RotateAsync(inboundJti, account.Id, newJti, utcNow.Add(refreshDuration)).UnauthorizedIfFalse();

			LoginResponse response = new()
			{
				AccessToken = newAccessToken,
				ExpiresAt = utcNow.Add(accessDuration),
			};

			transport.EmitToken(newRefreshToken, refreshDuration, response);

			return response;
		});
	}

	/// <summary>
	/// Validate that the account is still allowed to refresh its tokens.
	/// Throw a <see cref="DomainException" /> or <see cref="DomainHttpCodeException" /> to reject the refresh.
	/// </summary>
	protected abstract Task ValidateAccount(TAccount account);
}