using Storm.Api.Workers.Queues;

namespace Storm.Api.Logs.Sinks.ElasticSearch.Senders;

internal class ElasticSearchThrottleBufferSink : BaseElasticSearchWorkerSink
{
	public ElasticSearchThrottleBufferSink(ILogService service, IElasticSender client) : base(
		new BackgroundThrottledBufferedQueueWorker<string>(service, client.Send, TimeSpan.FromSeconds(5), null, 10, RetryStrategy))
	{
	}
}