using Microsoft.Extensions.Logging;
using Storm.Api.Logs.Interfaces;
using Storm.Api.Queues;

namespace Storm.Api.Logs.Sinks.ElasticSearch.Senders;

public class ElasticSearchHostedServiceLogSink : ThrottledBufferedItemQueue<string>, ILogSink
{
	public ElasticSearchHostedServiceLogSink() : this(100, TimeSpan.FromSeconds(10))
	{
	}

	public ElasticSearchHostedServiceLogSink(int bufferSize, TimeSpan throttlingTime) : base(bufferSize, throttlingTime)
	{
	}

	void ILogSink.Enqueue(LogLevel level, string entry)
	{
		Queue(entry);
	}
}