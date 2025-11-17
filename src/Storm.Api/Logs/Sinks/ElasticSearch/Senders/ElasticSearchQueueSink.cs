using Storm.Api.Workers.Queues;

namespace Storm.Api.Logs.Sinks.ElasticSearch.Senders;

internal class ElasticSearchQueueSink : BaseElasticSearchWorkerSink
{
	public ElasticSearchQueueSink(ILogService service, IElasticSender client) : base(
		new BackgroundQueueWorker<string>(service, client.Send, null, RetryStrategy))
	{
	}
}