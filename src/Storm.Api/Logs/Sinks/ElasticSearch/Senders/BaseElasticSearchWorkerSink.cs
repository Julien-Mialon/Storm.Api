using Microsoft.Extensions.Logging;
using Storm.Api.Logs.Interfaces;
using Storm.Api.Workers;
using Storm.Api.Workers.Strategies;

namespace Storm.Api.Logs.Sinks.ElasticSearch.Senders;

internal abstract class BaseElasticSearchWorkerSink : ILogSink
{
	protected static IRetryStrategy RetryStrategy => new ExponentialBackOffStrategy(5000, 4, 3);

	private readonly IWorker<string> _worker;

	protected BaseElasticSearchWorkerSink(IWorker<string> worker)
	{
		_worker = worker;
	}

	public void Enqueue(LogLevel level, string entry)
	{
		_worker.Queue(entry);
	}
}