using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Storm.Api.Authentications.Refresh.Transport;

public interface IRefreshTokenTransportResolver
{
	IRefreshTokenTransport Resolve();
}

internal class RefreshTokenTransportResolver : IRefreshTokenTransportResolver
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IServiceProvider _services;

	public RefreshTokenTransportResolver(IHttpContextAccessor httpContextAccessor, IServiceProvider services)
	{
		_httpContextAccessor = httpContextAccessor;
		_services = services;
	}

	public IRefreshTokenTransport Resolve()
	{
		object? raw = _httpContextAccessor.HttpContext?.Items[RefreshTokenTransportAttribute.HTTP_CONTEXT_KEY];

		if (raw is not RefreshTokenTransportMode mode)
		{
			throw new InvalidOperationException("RefreshTokenTransportMode not set. Apply [RefreshTokenTransport] to the controller or action method.");
		}

		return _services.GetRequiredKeyedService<IRefreshTokenTransport>(mode);
	}
}
