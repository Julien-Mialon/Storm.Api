using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Core.CQRS;

namespace Storm.Api.Authenticators;

public static class AuthenticatorsExtensions
{
	public static IServiceCollection AddConstantApiKeyAuthenticator<TAccount>(this IServiceCollection services, string apiKey, string headerName = "X-ApiKey", string queryParameterName = "ApiKey")
	{
		return services.AddScoped<IActionAuthenticator<TAccount, object>>(s =>
			new ConstantApiKeyAuthenticator<TAccount>(s, apiKey, headerName, queryParameterName));
	}
}