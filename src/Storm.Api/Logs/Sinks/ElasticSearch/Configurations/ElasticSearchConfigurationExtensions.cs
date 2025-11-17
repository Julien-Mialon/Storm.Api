using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Storm.Api.Extensions;

namespace Storm.Api.Logs.Sinks.ElasticSearch.Configurations;

public static class ElasticSearchConfigurationExtensions
{
	public static IElasticSearchConfigurationBuilder FromConfiguration(this IElasticSearchConfigurationBuilder builder, IConfiguration configuration)
	{
		/* Keys : Host ; Hosts ; User ; Password ; LogLevel ; Index */

		configuration.GetValue<string>("Host").Let(x => builder.WithNode(x));
		configuration.GetValue<string>("Hosts").Let(x => builder.WithNodes(x.Split(',', StringSplitOptions.RemoveEmptyEntries)));
		configuration.GetValue<string>("User")
			.Let(username => configuration.GetValue<string>("Password")
				.Let(password => builder.WithBasicAuthentication(username, password)));

		configuration.GetValue<string?>("LogLevel", null).LetParseEnum<LogLevel>(minimumLogLevel => builder.WithMinimumLogLevel(minimumLogLevel));
		configuration.GetValue<string?>("Index", null).Let(index => builder.WithIndex(index));

		return builder;
	}

	public static IElasticSearchConfigurationBuilder LoadElasticSearchConfiguration(this IConfiguration configuration)
	{
		/* Keys : Host ; Hosts ; User ; Password ; LogLevel ; Index */

		return ElasticSearchConfiguration.CreateBuilder()
			.FromConfiguration(configuration)
			.WithQueueSink();
	}
}