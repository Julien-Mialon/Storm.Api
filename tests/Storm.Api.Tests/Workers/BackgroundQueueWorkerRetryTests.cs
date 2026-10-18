using Storm.Api.Workers.Queues;
using Storm.Api.Workers.Strategies;

namespace Storm.Api.Tests.Workers;

public sealed class BackgroundQueueWorkerRetryTests : QueueWorkerRetryTests
{
	protected override Task<Action<string>> Start(Func<string, Task<bool>> action, IRetryStrategy retryStrategy)
	{
		// The worker exposes no way to stop it; it stays blocked on its empty queue after the test.
		BackgroundQueueWorker<string> worker = new(LogService, action, null, retryStrategy);
		return Task.FromResult<Action<string>>(worker.Queue);
	}
}
