using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.CQRS;
using Storm.Api.Extensions;

namespace Storm.Api.Authentications.ConstantApiKeys;

public abstract class BaseConstantApiKeyAuthenticator<TAccount> : BaseServiceContainer, IActionAuthenticator<TAccount>
{
	private readonly string _apiKey;
	private readonly string? _headerName;
	private readonly string? _queryParameterName;

	protected BaseConstantApiKeyAuthenticator(IServiceProvider services, string apiKey, string? headerName, string? queryParameterName) : base(services)
	{
		_apiKey = apiKey;
		_headerName = headerName;
		_queryParameterName = queryParameterName;
	}

	public Task<TAccount?> Authenticate()
	{
		IHttpContextAccessor context = Services.GetRequiredService<IHttpContextAccessor>();

		string? apiKey = null;
		if (_headerName is not null && context.TryGetHeader(_headerName, out string? headerValue))
		{
			apiKey = headerValue;
		}
		else if (_queryParameterName is not null && context.TryGetQueryParameter(_queryParameterName, out string? queryParameterValue))
		{
			apiKey = queryParameterValue;
		}
		else
		{
			return Task.FromResult<TAccount?>(default);
		}

		if (apiKey == _apiKey)
		{
			return Task.FromResult(CreateAccount());
		}

		return Task.FromResult<TAccount?>(default);
	}

	protected abstract TAccount? CreateAccount();
}