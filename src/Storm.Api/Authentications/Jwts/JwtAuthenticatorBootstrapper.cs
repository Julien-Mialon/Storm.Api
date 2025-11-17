using Microsoft.Extensions.DependencyInjection;
using Storm.Api.CQRS;
using Storm.Api.Databases.Models;

namespace Storm.Api.Authentications.Jwts;

public static class JwtAuthenticatorBootstrapper
{
	public static IServiceCollection AddJwtSupportServices(this IServiceCollection services)
	{
		services.AddSingleton<IJwtTokenService, JwtTokenService>();

		return services;
	}

	public static IServiceCollection AddJwtAuthenticator<TAccount>(this IServiceCollection services, JwtConfiguration<TAccount> configuration, string headerName = "Authorization", string queryParameterName = "Authorization", string tokenType = "Bearer")
		where TAccount : IGuidEntity
	{
		services.AddJwtSupportServices();

		services.AddSingleton(configuration)
			.AddSingleton<JwtService<TAccount>>()
			.AddScoped<IActionAuthenticator<TAccount>>(s => new JwtAuthenticator<TAccount>(s, headerName, queryParameterName, tokenType));

		return services;
	}

	public static IServiceCollection AddRawJwtAuthenticator(this IServiceCollection services, JwtConfiguration<Guid> configuration, string headerName = "Authorization", string queryParameterName = "Authorization", string tokenType = "Bearer")
	{
		services.AddJwtSupportServices();

		services.AddSingleton(configuration)
			.AddSingleton<JwtService<Guid>>()
			.AddSingleton<IActionAuthenticator<Guid?>>(s => new JwtRawAuthenticator(s, headerName, queryParameterName, tokenType));

		return services;
	}
}