using Elasticsearch.Net;
using Storm.Api.Core.Logs.ElasticSearch.Senders;

namespace Storm.Api.Core.Logs.ElasticSearch.Configurations;

public class ElasticSearchConfiguration
{
	private readonly List<string> _nodes = new();
	private string? _username;
	private string? _password;
	private Func<IElasticSender, ILogService, ILogSender>? _senderFactory;
	private string? _index;

	internal LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;

	internal void AddNode(string node) => _nodes.Add(node);

	internal void AddNodes(IEnumerable<string> nodes) => _nodes.AddRange(nodes);

	internal void UseBasicAuthentication(string username, string password)
	{
		_username = username;
		_password = password;
	}

	internal void UseSender(Func<IElasticSender, ILogService, ILogSender> senderFactory)
	{
		_senderFactory = senderFactory;
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

		return new LogService(logService => _senderFactory!(CreateElasticSender(), logService), MinimumLogLevel);
	}

	public LogQueueService CreateQueueService()
	{
		if (_nodes.Count == 0)
		{
			throw new InvalidOperationException("You must specify at least one node address");
		}

		return new(MinimumLogLevel);
	}

	public IElasticSender CreateElasticSender()
	{
		ConnectionConfiguration configuration;
		if (_nodes.Count == 1)
		{
			configuration = new(new Uri(_nodes[0]));
		}
		else
		{
			configuration = new(new StaticConnectionPool(_nodes.ConvertAll(x => new Uri(x))));
		}

		if (_username is not null)
		{
			configuration = configuration.BasicAuthentication(_username, _password);
		}

		configuration.ConnectionLimit(-1);
		ElasticLowLevelClient client = new(configuration);
		ElasticSender sender = new(client, _index!);

		return sender;
	}

	public static IElasticSearchConfigurationBuilder CreateBuilder() => new ElasticSearchConfigurationBuilder();
}