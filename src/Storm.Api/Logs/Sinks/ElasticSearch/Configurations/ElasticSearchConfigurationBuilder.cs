using Microsoft.Extensions.Logging;
using Storm.Api.Logs.Interfaces;
using Storm.Api.Logs.Sinks.ElasticSearch.Senders;

namespace Storm.Api.Logs.Sinks.ElasticSearch.Configurations;

internal class ElasticSearchConfigurationBuilder : IElasticSearchConfigurationBuilder
{
	private ElasticSearchConfiguration? _configuration = new();
	private ElasticSearchConfiguration Configuration => _configuration ?? throw new InvalidOperationException("You can not change parameters in builder after calling Build()");

	public ElasticSearchConfiguration Build()
	{
		ElasticSearchConfiguration configuration = Configuration;
		_configuration = null;
		return configuration;
	}

	public IElasticSearchConfigurationBuilder WithBasicAuthentication(string username, string password)
	{
		Configuration.UseBasicAuthentication(username, password);
		return this;
	}

	public IElasticSearchConfigurationBuilder WithNode(string node)
	{
		Configuration.AddNode(node);
		return this;
	}

	public IElasticSearchConfigurationBuilder WithNodes(params string[] nodes) => WithNodes((IEnumerable<string>)nodes);

	public IElasticSearchConfigurationBuilder WithNodes(IEnumerable<string> nodes)
	{
		Configuration.AddNodes(nodes);
		return this;
	}

	public IElasticSearchConfigurationBuilder WithMinimumLogLevel(LogLevel minimumLogLevel)
	{
		Configuration.MinimumLogLevel = minimumLogLevel;
		return this;
	}

	public IElasticSearchConfigurationBuilder WithIndex(string index)
	{
		Configuration.UseIndex(index);
		return this;
	}

	public IElasticSearchConfigurationBuilder WithSink(Func<IElasticSender, ILogService, ILogSink> sinkFactory)
	{
		Configuration.UseSender(sinkFactory);
		return this;
	}

	public IElasticSearchConfigurationBuilder WithQueueSink()
	{
		Configuration.UseSender((client, logService) => new ElasticSearchQueueSink(logService, client));
		return this;
	}

	public IElasticSearchConfigurationBuilder WithBufferSink()
	{
		Configuration.UseSender((client, logService) => new ElasticSearchBufferSink(logService, client));
		return this;
	}

	public IElasticSearchConfigurationBuilder WithThrottleBufferSink()
	{
		Configuration.UseSender((client, logService) => new ElasticSearchThrottleBufferSink(logService, client));
		return this;
	}
}