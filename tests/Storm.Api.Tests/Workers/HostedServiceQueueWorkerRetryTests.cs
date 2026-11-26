using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Queues;
using Storm.Api.Workers.HostedServices;
using Storm.Api.Workers.Strategies;
using Xunit;

namespace Storm.Api.Tests.Workers;

public sealed class HostedServiceQueueWorkerRetryTests : QueueWorkerRetryTests, IDisposable
{
	private ServiceProvider? _services;
	private DelegatingWorker? _worker;

	protected override Task<Action<string>> Start(Func<string, Task<bool>> action, IRetryStrategy retryStrategy)
	{
		return StartWorker((item, _) => action(item), retryStrategy);
	}

	[Fact]
	public async Task Stopping_cancels_the_pending_retry_wait()
	{
		TaskCompletionSource attempted = new(TaskCreationOptions.RunContinuationsAsynchronously);
		Action<string> queue = await StartWorker((_, _) =>
		{
			attempted.TrySetResult();
			return Task.FromResult(false);
		}, new DelayRetryStrategy(Timeout.Infinite, null));
		queue("first");
		await attempted.Task.WaitAsync(STEP_TIMEOUT);

		await StopWorker();
	}

	[Fact]
	public async Task Stopping_cancels_the_running_action_without_logging_it()
	{
		TaskCompletionSource attempted = new(TaskCreationOptions.RunContinuationsAsynchronously);
		Action<string> queue = await StartWorker(async (_, ct) =>
		{
			attempted.TrySetResult();
			await Task.Delay(Timeout.Infinite, ct);
			return true;
		}, new DelayRetryStrategy(Timeout.Infinite, null));
		queue("first");
		await attempted.Task.WaitAsync(STEP_TIMEOUT);

		await StopWorker();

		Assert.Empty(LogEntries);
	}

	public void Dispose()
	{
		_worker?.Dispose();
		_services?.Dispose();
	}

	private async Task<Action<string>> StartWorker(Func<string, CancellationToken, Task<bool>> action, IRetryStrategy retryStrategy)
	{
		_services = new ServiceCollection()
			.AddSingleton(LogService)
			.AddSingleton<ItemQueue<string>>()
			.BuildServiceProvider();

		_worker = new(_services, retryStrategy, action);
		await _worker.StartAsync(CancellationToken.None);

		return _services.GetRequiredService<ItemQueue<string>>().Queue;
	}

	private async Task StopWorker()
	{
		using CancellationTokenSource timeout = new(STEP_TIMEOUT);
		await _worker!.StopAsync(timeout.Token);

		// StopAsync gives up silently once its token is cancelled, so check that the loop really ended.
		Assert.True(_worker.ExecuteTask?.IsCompleted);
	}

	private sealed class DelegatingWorker(IServiceProvider services, IRetryStrategy retryStrategy, Func<string, CancellationToken, Task<bool>> action)
		: AbstractHostedServiceQueueWorker<string, string, ItemQueue<string>>(services, retryStrategy)
	{
		protected override Task<bool> DoAction(string item, CancellationToken ct)
		{
			return action(item, ct);
		}
	}
}
