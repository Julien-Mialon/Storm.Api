using System;

namespace Storm.Api.Core.Services
{
	public interface IScopeServiceAccessor
	{
		TService Get<TService>();

		IServiceProvider Services { get; }
	}
}