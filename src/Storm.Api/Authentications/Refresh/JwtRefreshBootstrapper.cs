using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Storm.Api.Authentications.Jwts;
using Storm.Api.Authentications.Refresh.Database;
using Storm.Api.Authentications.Refresh.Handlers;
using Storm.Api.Databases.Models;

namespace Storm.Api.Authentications.Refresh;

public static class JwtRefreshBootstrapper
{
	/// <summary>
	/// Registers cookie-based refresh token support (HttpOnly cookie + optional CSRF).
	/// Apply <see cref="RefreshTokenModeAttribute" /> with <see cref="RefreshTokenMode.Cookie" />
	/// on controllers that should use this mode.
	/// </summary>
	public static IServiceCollection AddCookieRefreshTokens<TAccount>(this IServiceCollection services, JwtConfiguration<RefreshTokenMarker> refreshTokenConfiguration, JwtRefreshCookieConfiguration cookieConfiguration) where TAccount : IGuidEntity
	{
		AddCommon(services, refreshTokenConfiguration);

		services.AddSingleton(cookieConfiguration);
		services.TryAddScoped<IJwtRefreshCookieService, JwtRefreshCookieService>();
		services.AddKeyedScoped<IRefreshTokenHandler, CookieRefreshTokenHandler>(RefreshTokenMode.Cookie);

		return services;
	}

	/// <summary>
	/// Registers database-backed refresh token support (token in response body, JTI stored in DB).
	/// Apply <see cref="RefreshTokenModeAttribute" /> with <see cref="RefreshTokenMode.Database" />
	/// on controllers that should use this mode.
	/// </summary>
	public static IServiceCollection AddDatabaseRefreshTokens<TAccount>(this IServiceCollection services, JwtConfiguration<RefreshTokenMarker> refreshTokenConfiguration) where TAccount : IGuidEntity
	{
		AddCommon(services, refreshTokenConfiguration);

		services.TryAddScoped<IRefreshTokenStore, RefreshTokenStore>();
		services.AddKeyedScoped<IRefreshTokenHandler, DatabaseRefreshTokenHandler>(RefreshTokenMode.Database);

		return services;
	}

	/// <summary>
	/// Registers both cookie and database refresh token support.
	/// Use <see cref="RefreshTokenModeAttribute" /> on controllers to select which mode to use.
	/// </summary>
	public static IServiceCollection AddAllRefreshTokens<TAccount>(this IServiceCollection services, JwtConfiguration<RefreshTokenMarker> refreshTokenConfiguration, JwtRefreshCookieConfiguration cookieConfiguration) where TAccount : IGuidEntity
	{
		services.AddCookieRefreshTokens<TAccount>(refreshTokenConfiguration, cookieConfiguration);
		services.AddDatabaseRefreshTokens<TAccount>(refreshTokenConfiguration);
		return services;
	}

	private static void AddCommon(IServiceCollection services, JwtConfiguration<RefreshTokenMarker> refreshTokenConfiguration)
	{
		services.AddJwtSupportServices();
		services.TryAddSingleton(refreshTokenConfiguration);
		services.TryAddSingleton<JwtService<RefreshTokenMarker>>();
		services.TryAddScoped<IRefreshTokenHandlerResolver, RefreshTokenHandlerResolver>();
		services.AddHttpContextAccessor();
	}
}