using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Core.Services;

namespace Storm.Api.Services
{
	public class ScopeServiceAccessor : IScopeServiceAccessor
	{
		private readonly IHttpContextAccessor _accessor;

		public ScopeServiceAccessor(IHttpContextAccessor contextAccessor)
		{
			_accessor = contextAccessor;
		}

		public IServiceProvider Services => _accessor.HttpContext.RequestServices;

		public TService Get<TService>() => _accessor.HttpContext.RequestServices.GetService<TService>();
	}
}