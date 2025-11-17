using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.CQRS;
using Storm.Api.Extensions;

namespace Storm.Api.Authentications.Commons;

public abstract class BaseTokenAuthenticator<TAccount> : BaseServiceContainer, IActionAuthenticator<TAccount>
{
	private readonly string? _tokenType;
	private readonly string? _headerName;
	private readonly string? _queryParameterName;

	protected BaseTokenAuthenticator(IServiceProvider services, string? headerName, string? queryParameterName, string? tokenType) : base(services)
	{
		_headerName = headerName;
		_queryParameterName = queryParameterName;
		_tokenType = tokenType;
	}

	public Task<TAccount?> Authenticate()
	{
		IHttpContextAccessor context = Services.GetRequiredService<IHttpContextAccessor>();

		string? tokenValue = null;
		if (_headerName is not null && context.TryGetHeader(_headerName, out string? headerValue))
		{
			tokenValue = headerValue;
		}
		else if (_queryParameterName is not null && context.TryGetQueryParameter(_queryParameterName, out string? queryParameterValue))
		{
			tokenValue = queryParameterValue;
		}
		else
		{
			return Task.FromResult<TAccount?>(default);
		}

		if (_tokenType is null)
		{
			return Authenticate(tokenValue);
		}

		if (tokenValue.StartsWith(_tokenType))
		{
			return Authenticate(tokenValue[(_tokenType.Length + 1)..]);
		}

		return Task.FromResult<TAccount?>(default);
	}

	protected abstract Task<TAccount?> Authenticate(string token);
}