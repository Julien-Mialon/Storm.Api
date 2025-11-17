using Storm.Api.Workers.Queues;

namespace Storm.Api.Logs.Sinks.ElasticSearch.Senders;

internal class ElasticSearchBufferSink : BaseElasticSearchWorkerSink
{
	public ElasticSearchBufferSink(ILogService service, IElasticSender client) : base(
		new BackgroundBufferedQueueWorker<string>(service, client.Send, null, 10, RetryStrategy))
	{
	}
}