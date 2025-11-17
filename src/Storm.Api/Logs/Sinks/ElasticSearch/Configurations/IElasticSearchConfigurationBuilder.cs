using Microsoft.Extensions.Logging;
using Storm.Api.Logs.Interfaces;
using Storm.Api.Logs.Sinks.ElasticSearch.Senders;

namespace Storm.Api.Logs.Sinks.ElasticSearch.Configurations;

public interface IElasticSearchConfigurationBuilder
{
	ElasticSearchConfiguration Build();
	IElasticSearchConfigurationBuilder WithBasicAuthentication(string username, string password);
	IElasticSearchConfigurationBuilder WithNode(string node);
	IElasticSearchConfigurationBuilder WithNodes(params string[] nodes);
	IElasticSearchConfigurationBuilder WithNodes(IEnumerable<string> nodes);
	IElasticSearchConfigurationBuilder WithMinimumLogLevel(LogLevel minimumLogLevel);
	IElasticSearchConfigurationBuilder WithIndex(string index);
	IElasticSearchConfigurationBuilder WithSink(Func<IElasticSender, ILogService, ILogSink> sinkFactory);
	IElasticSearchConfigurationBuilder WithQueueSink();
	IElasticSearchConfigurationBuilder WithBufferSink();
	IElasticSearchConfigurationBuilder WithThrottleBufferSink();
}