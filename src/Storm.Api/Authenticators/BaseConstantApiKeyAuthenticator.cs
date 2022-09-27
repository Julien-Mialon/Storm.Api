using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Core.CQRS;
using Storm.Api.Core.Services;
using Storm.Api.Extensions;

namespace Storm.Api.Authenticators;

public abstract class BaseConstantApiKeyAuthenticator<TAccount> : BaseService, IActionAuthenticator<TAccount, object>
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

	public Task<(bool authenticated, TAccount? account)> Authenticate(object parameter)
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
			return Task.FromResult<(bool, TAccount?)>((false, default));
		}

		if (apiKey == _apiKey)
		{
			return Task.FromResult<(bool, TAccount?)>((true, CreateAccount()));
		}

		return Task.FromResult<(bool, TAccount?)>((false, default));
	}

	protected abstract TAccount? CreateAccount();
}