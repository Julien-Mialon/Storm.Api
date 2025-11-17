using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Storm.Api.Services;

public class ScopedServiceAccessor : IScopedServiceAccessor
{
	private readonly IHttpContextAccessor _accessor;

	public ScopedServiceAccessor(IHttpContextAccessor contextAccessor)
	{
		_accessor = contextAccessor;
	}

	public IServiceProvider Services
	{
		get
		{
			if (_accessor.HttpContext is { } context)
			{
				return context.RequestServices;
			}

			throw new InvalidOperationException("HttpContextAccessor is not available here");
		}
	}

	public TService Get<TService>() where TService : notnull
		=> Services.GetRequiredService<TService>();
}