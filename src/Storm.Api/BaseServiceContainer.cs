using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Storm.Api;

public abstract class BaseServiceContainer
{
	protected IServiceProvider Services { get; }

	protected BaseServiceContainer(IServiceProvider services)
	{
		Services = services;
	}

	protected TService Resolve<TService>([CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
		where TService : class
	{
		TService service = Services.GetRequiredService<TService>();

		if (service is null)
		{
			throw new InvalidOperationException($"Can not resolve instance of service of type {typeof(TService).Name} from {member} in {file}:{line}");
		}

		return service;
	}

	protected TService ResolveKeyed<TService>(object key, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
		where TService : class
	{
		TService service = Services.GetRequiredKeyedService<TService>(key);

		if (service is null)
		{
			throw new InvalidOperationException($"Can not resolve instance of service of type {typeof(TService).Name} from {member} in {file}:{line}");
		}

		return service;
	}
}