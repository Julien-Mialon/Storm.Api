using Microsoft.Extensions.Hosting;
using Storm.Api.Extensions;

namespace Storm.Api.Workers.HostedServices;

public class MultipleInstanceHostedService<THostedService> : IHostedService, IDisposable
	where THostedService : IHostedService, IDisposable
{
	private readonly List<THostedService> _services = [];

	public MultipleInstanceHostedService(IServiceProvider services, int count)
	{
		for (int i = 0; i < count; i++)
		{
			_services.Add(services.Create<THostedService>());
		}
	}

	public MultipleInstanceHostedService(IServiceProvider services, int count, Func<IServiceProvider, THostedService> factory)
	{
		for (int i = 0; i < count; i++)
		{
			_services.Add(factory(services));
		}
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		foreach (THostedService service in _services)
		{
			service.StartAsync(cancellationToken);
		}

		return Task.CompletedTask;
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		List<Task> tasks = new(_services.Capacity);
		foreach (THostedService service in _services)
		{
			tasks.Add(service.StopAsync(cancellationToken));
		}

		await Task.WhenAll(tasks);
	}

	public void Dispose()
	{
		foreach (THostedService service in _services)
		{
			service.Dispose();
		}
		_services.Clear();
	}
}