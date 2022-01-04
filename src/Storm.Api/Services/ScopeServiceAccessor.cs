using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Core.Services;

namespace Storm.Api.Services;

public class ScopeServiceAccessor : IScopeServiceAccessor
{
	private readonly IHttpContextAccessor _accessor;

	public ScopeServiceAccessor(IHttpContextAccessor contextAccessor)
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