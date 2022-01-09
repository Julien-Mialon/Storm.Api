using Storm.Api.Core.Logs.ElasticSearch.Senders;

namespace Storm.Api.Core.Logs.ElasticSearch.Configurations;

public interface IElasticSearchConfigurationBuilder
{
	ElasticSearchConfiguration Build();
	IElasticSearchConfigurationBuilder WithBasicAuthentication(string username, string password);
	IElasticSearchConfigurationBuilder WithNode(string node);
	IElasticSearchConfigurationBuilder WithNodes(params string[] nodes);
	IElasticSearchConfigurationBuilder WithNodes(IEnumerable<string> nodes);
	IElasticSearchConfigurationBuilder WithMinimumLogLevel(LogLevel minimumLogLevel);
	IElasticSearchConfigurationBuilder WithSender(Func<IElasticSender, ILogService, ILogSender> senderFactory);
	IElasticSearchConfigurationBuilder WithImmediateSender();
	IElasticSearchConfigurationBuilder WithIndex(string index);
}

internal class ElasticSearchConfigurationBuilder : IElasticSearchConfigurationBuilder
{
	private ElasticSearchConfiguration _configuration = new();
	private ElasticSearchConfiguration Configuration => _configuration ?? throw new InvalidOperationException("You can not change parameters in builder after calling Build()");

	public ElasticSearchConfiguration Build()
	{
		ElasticSearchConfiguration configuration = Configuration;
		_configuration = new();
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

	public IElasticSearchConfigurationBuilder WithSender(Func<IElasticSender, ILogService, ILogSender> senderFactory)
	{
		Configuration.UseSender(senderFactory);
		return this;
	}

	public IElasticSearchConfigurationBuilder WithImmediateSender()
	{
		Configuration.UseSender((client, logService) => new ImmediateQueueLogSender(logService, client));
		return this;
	}

	public IElasticSearchConfigurationBuilder WithIndex(string index)
	{
		Configuration.UseIndex(index);
		return this;
	}
}