using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Storm.Api.Authentications.Refresh;

public interface IRefreshTokenHandlerResolver
{
	IRefreshTokenHandler Resolve();
}

internal class RefreshTokenHandlerResolver : IRefreshTokenHandlerResolver
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IServiceProvider _services;

	public RefreshTokenHandlerResolver(IHttpContextAccessor httpContextAccessor, IServiceProvider services)
	{
		_httpContextAccessor = httpContextAccessor;
		_services = services;
	}

	public IRefreshTokenHandler Resolve()
	{
		object? raw = _httpContextAccessor.HttpContext?.Items[RefreshTokenModeAttribute.HttpContextKey];

		if (raw is not RefreshTokenMode mode)
		{
			throw new InvalidOperationException("RefreshTokenMode not set. Apply [RefreshTokenMode] to the controller or action method.");
		}

		return _services.GetRequiredKeyedService<IRefreshTokenHandler>(mode);
	}
}