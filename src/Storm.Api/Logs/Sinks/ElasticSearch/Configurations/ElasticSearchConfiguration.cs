using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Logging;
using Storm.Api.Logs.Interfaces;
using Storm.Api.Logs.Sinks.ElasticSearch.Senders;

namespace Storm.Api.Logs.Sinks.ElasticSearch.Configurations;

public class ElasticSearchConfiguration
{
	private readonly List<string> _nodes = new();
	private string? _username;
	private string? _password;
	private Func<IElasticSender, ILogService, ILogSink>? _sinkFactory;
	private string? _index;

	internal LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

	internal void AddNode(string node) => _nodes.Add(node);

	internal void AddNodes(IEnumerable<string> nodes) => _nodes.AddRange(nodes);

	internal void UseBasicAuthentication(string username, string password)
	{
		_username = username;
		_password = password;
	}

	internal void UseSender(Func<IElasticSender, ILogService, ILogSink> senderFactory)
	{
		_sinkFactory = senderFactory;
	}

	internal void UseIndex(string index)
	{
		_index = index;
	}

	public ILogService CreateService()
	{
		if (_nodes.Count == 0)
		{
			throw new InvalidOperationException("You must specify at least one node address");
		}

		return new LogService(logService => _sinkFactory!(CreateElasticSender(), logService), MinimumLogLevel);
	}

	public IElasticSender CreateElasticSender()
	{
		ElasticsearchClientSettings configuration;
		if (_nodes.Count == 1)
		{
			configuration = new(new Uri(_nodes[0]));
		}
		else
		{
			configuration = new(new StaticNodePool(_nodes.ConvertAll(x => new Uri(x))));
		}

		if (_username is not null && _password is not null)
		{
			configuration = configuration.Authentication(new BasicAuthentication(_username, _password));
		}

		configuration.ConnectionLimit(-1);
		ElasticsearchClient client = new(configuration);
		ElasticLogSender logSender = new(client, _index!);

		return logSender;
	}

	public static IElasticSearchConfigurationBuilder CreateBuilder() => new ElasticSearchConfigurationBuilder();
}