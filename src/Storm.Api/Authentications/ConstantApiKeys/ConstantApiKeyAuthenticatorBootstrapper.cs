using Microsoft.Extensions.DependencyInjection;
using Storm.Api.CQRS;

namespace Storm.Api.Authentications.ConstantApiKeys;

public static class ConstantApiKeyAuthenticatorBootstrapper
{
	public static IServiceCollection AddConstantApiKeyAuthenticator<TAccount>(this IServiceCollection services, string apiKey, string headerName = "X-ApiKey", string queryParameterName = "ApiKey")
		where TAccount : new()
	{
		return services.AddSingleton<IActionAuthenticator<TAccount>>(s =>
			new ConstantApiKeyAuthenticator<TAccount>(s, apiKey, headerName, queryParameterName));
	}
}