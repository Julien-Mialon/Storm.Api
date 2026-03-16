using System.Net;
using Storm.Api.Authentications.Jwts;
using Storm.Api.CQRS;
using Storm.Api.CQRS.Exceptions;
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
		IRefreshTokenHandler handler = Resolve<IRefreshTokenHandlerResolver>().Resolve();

		string? inboundToken = handler.ReadInboundToken(parameter);
		if (inboundToken is null)
		{
			throw new DomainHttpCodeException(HttpStatusCode.Unauthorized);
		}

		if (!handler.ValidateTransport(inboundToken))
		{
			throw new DomainHttpCodeException(HttpStatusCode.Unauthorized);
		}

		JwtService<RefreshTokenMarker> refreshSvc = Resolve<JwtService<RefreshTokenMarker>>();

		if (!refreshSvc.TryValidateToken(inboundToken, out Guid accountId))
		{
			await handler.RevokeAsync(inboundToken, null);
			throw new DomainHttpCodeException(HttpStatusCode.Unauthorized);
		}

		string? inboundJti = JtiExtractor.Extract(inboundToken);
		if (inboundJti is null)
		{
			throw new DomainHttpCodeException(HttpStatusCode.Unauthorized);
		}

		if (!await handler.ValidateTokenStateAsync(inboundToken, inboundJti))
		{
			throw new DomainHttpCodeException(HttpStatusCode.Unauthorized);
		}

		TAccount? account = await Resolve<IGuidRepository<TAccount>>().GetById(accountId);
		if (account is null)
		{
			await handler.RevokeAsync(inboundToken, inboundJti);
			throw new DomainHttpCodeException(HttpStatusCode.Unauthorized);
		}

		await ValidateAccount(account);

		// Rotate: revoke old token and issue new ones
		await handler.RevokeAsync(inboundToken, inboundJti);

		JwtService<TAccount> accessSvc = Resolve<JwtService<TAccount>>();
		(string newAccessToken, TimeSpan accessDuration) = accessSvc.GenerateToken(account.Id);

		string newJti = Guid.NewGuid().ToString("N");
		(string newRefreshToken, TimeSpan refreshDuration) = refreshSvc.GenerateToken(account.Id, new Dictionary<string, string>
		{
			["jti"] = newJti,
		});

		LoginResponse response = new()
		{
			AccessToken = newAccessToken,
			ExpiresAt = DateTime.UtcNow.Add(accessDuration),
		};

		await handler.StoreAndEmitAsync(account.Id, newRefreshToken, newJti, refreshDuration, response);

		return response;
	}

	/// <summary>
	/// Validate that the account is still allowed to refresh its tokens.
	/// Throw a <see cref="Storm.Api.CQRS.Exceptions.DomainException" /> or
	/// <see cref="Storm.Api.CQRS.Exceptions.DomainHttpCodeException" /> to reject the refresh.
	/// </summary>
	protected abstract Task ValidateAccount(TAccount account);
}