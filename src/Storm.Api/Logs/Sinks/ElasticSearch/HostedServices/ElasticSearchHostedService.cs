using Storm.Api.Logs.Sinks.ElasticSearch.Senders;
using Storm.Api.Workers.HostedServices;
using Storm.Api.Workers.Strategies;

namespace Storm.Api.Logs.Sinks.ElasticSearch.HostedServices;

public class ElasticSearchHostedService : AbstractHostedServiceQueueWorker<string, string[], ElasticSearchHostedServiceLogSink>
{
	private IElasticSender? _sender;

	public ElasticSearchHostedService(IServiceProvider services)
		: base(services, new ExponentialBackOffStrategy(5000, 4, 3))
	{
	}

	protected override async Task<bool> DoAction(string[] item)
	{
		_sender ??= Resolve<IElasticSender>();
		return await _sender.Send(item);
	}
}