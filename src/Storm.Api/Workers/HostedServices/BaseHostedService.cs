using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Storm.Api.Workers.HostedServices;

public abstract class BaseHostedService : BackgroundService
{
	protected IServiceProvider Services { get; }

	protected BaseHostedService(IServiceProvider services)
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
}