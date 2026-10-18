using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Queues;
using Storm.Api.Workers.HostedServices;
using Storm.Api.Workers.Strategies;

namespace Storm.Api.Tests.Workers;

public sealed class HostedServiceQueueWorkerRetryTests : QueueWorkerRetryTests, IDisposable
{
	private ServiceProvider? _services;
	private DelegatingWorker? _worker;

	protected override async Task<Action<string>> Start(Func<string, Task<bool>> action, IRetryStrategy retryStrategy)
	{
		_services = new ServiceCollection()
			.AddSingleton(LogService)
			.AddSingleton<ItemQueue<string>>()
			.BuildServiceProvider();

		_worker = new(_services, retryStrategy, action);
		await _worker.StartAsync(CancellationToken.None);

		return _services.GetRequiredService<ItemQueue<string>>().Queue;
	}

	public void Dispose()
	{
		_worker?.Dispose();
		_services?.Dispose();
	}

	private sealed class DelegatingWorker(IServiceProvider services, IRetryStrategy retryStrategy, Func<string, Task<bool>> action)
		: AbstractHostedServiceQueueWorker<string, string, ItemQueue<string>>(services, retryStrategy)
	{
		protected override Task<bool> DoAction(string item)
		{
			return action(item);
		}
	}
}
