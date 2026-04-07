using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Storm.Api.Authentications.Jwts;
using Storm.Api.Authentications.Refresh.Storage;
using Storm.Api.Authentications.Refresh.Transport;
using Storm.Api.Databases.Models;

namespace Storm.Api.Authentications.Refresh;

public static class RefreshTokenBootstrapper
{
	/// <summary>
	/// Registers refresh token infrastructure with a fluent builder for storage and transport selection.
	/// </summary>
	public static RefreshTokenBuilder AddRefreshTokens<TAccount>(this IServiceCollection services, JwtConfiguration<RefreshTokenMarker> refreshTokenConfiguration)
		where TAccount : IGuidEntity
	{
		services.AddJwtSupportServices();
		services.TryAddSingleton(refreshTokenConfiguration);
		services.TryAddSingleton<JwtService<RefreshTokenMarker>>();
		services.TryAddScoped<IRefreshTokenTransportResolver, RefreshTokenTransportResolver>();
		services.AddHttpContextAccessor();

		return new RefreshTokenBuilder(services);
	}
}

public class RefreshTokenBuilder
{
	private readonly IServiceCollection _services;

	internal RefreshTokenBuilder(IServiceCollection services)
	{
		_services = services;
	}

	/// <summary>
	/// Uses stateless JWT-based refresh token storage (no server-side state, no revocation support).
	/// </summary>
	public RefreshTokenBuilder WithJwtStorage()
	{
		_services.TryAddSingleton<IRefreshTokenStorage, JwtRefreshTokenStorage>();
		return this;
	}

	/// <summary>
	/// Uses database-backed refresh token storage (JTI stored in DB, supports revocation).
	/// </summary>
	public RefreshTokenBuilder WithDatabaseStorage()
	{
		_services.TryAddScoped<IRefreshTokenStorage, DatabaseRefreshTokenStorage>();
		return this;
	}

	/// <summary>
	/// Registers JSON body transport for refresh tokens.
	/// Apply <see cref="RefreshTokenTransportAttribute" /> with <see cref="RefreshTokenTransportMode.Json" />
	/// on controllers that should use this mode.
	/// </summary>
	public RefreshTokenBuilder WithJsonTransport()
	{
		_services.AddKeyedSingleton<IRefreshTokenTransport, JsonRefreshTokenTransport>(RefreshTokenTransportMode.Json);
		return this;
	}

	/// <summary>
	/// Registers cookie-based transport for refresh tokens (HttpOnly cookie + CSRF).
	/// Apply <see cref="RefreshTokenTransportAttribute" /> with <see cref="RefreshTokenTransportMode.Cookie" />
	/// on controllers that should use this mode.
	/// </summary>
	public RefreshTokenBuilder WithCookieTransport(CookieTransportConfiguration configuration)
	{
		_services.AddSingleton(configuration);
		_services.AddKeyedScoped<IRefreshTokenTransport, CookieRefreshTokenTransport>(RefreshTokenTransportMode.Cookie);
		return this;
	}
}
