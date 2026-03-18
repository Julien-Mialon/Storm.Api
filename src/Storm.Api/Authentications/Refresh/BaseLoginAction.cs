using System.Net;
using Storm.Api.Authentications.Jwts;
using Storm.Api.CQRS;
using Storm.Api.CQRS.Exceptions;
using Storm.Api.Databases.Models;
using Storm.Api.Dtos;

namespace Storm.Api.Authentications.Refresh;

public abstract class BaseLoginAction<TParameter, TAccount>(IServiceProvider services)
	: BaseAction<TParameter, LoginResponse>(services)
	where TAccount : IGuidEntity
{
	protected sealed override async Task<LoginResponse> Action(TParameter parameter)
	{
		TAccount? account = await AuthenticateCredentials(parameter);
		if (account is null)
		{
			throw new DomainHttpCodeException(HttpStatusCode.Unauthorized);
		}

		IRefreshTokenHandler handler = Resolve<IRefreshTokenHandlerResolver>().Resolve();
		JwtService<TAccount> accessSvc = Resolve<JwtService<TAccount>>();
		JwtService<RefreshTokenMarker> refreshSvc = Resolve<JwtService<RefreshTokenMarker>>();

		(string accessToken, TimeSpan accessDuration) = accessSvc.GenerateToken(account.Id);

		string jti = Guid.NewGuid().ToString("N");
		(string refreshToken, TimeSpan refreshDuration) = refreshSvc.GenerateToken(account.Id, new Dictionary<string, string>
		{
			["jti"] = jti,
		});

		LoginResponse response = new()
		{
			AccessToken = accessToken,
			ExpiresAt = Resolve<TimeProvider>().GetUtcNow().UtcDateTime.Add(accessDuration),
		};

		await handler.StoreAndEmitAsync(account.Id, refreshToken, jti, refreshDuration, response);

		return response;
	}

	/// <summary>
	/// Validate credentials and return the matching account, or null if invalid.
	/// </summary>
	protected abstract Task<TAccount?> AuthenticateCredentials(TParameter parameter);
}