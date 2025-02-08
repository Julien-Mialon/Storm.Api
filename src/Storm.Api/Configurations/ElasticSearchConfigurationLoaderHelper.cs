using Microsoft.Extensions.Configuration;
using Storm.Api.Core.Logs;
using Storm.Api.Core.Logs.ElasticSearch.Configurations;

namespace Storm.Api.Configurations;

public static class ElasticSearchConfigurationLoaderHelper
{
	public static IElasticSearchConfigurationBuilder FromConfiguration(this IElasticSearchConfigurationBuilder builder, IConfiguration configuration)
	{
		/* Keys : Host ; User ; Password ; LogLevel ; Index */

		builder = builder
			.WithNode(configuration.GetValue<string>("Host")!)
			.WithBasicAuthentication(configuration.GetValue<string>("User")!, configuration.GetValue<string>("Password")!);

		if (configuration.GetValue<string?>("LogLevel", null) is { } minimumLogLevelString && Enum.TryParse(minimumLogLevelString, out LogLevel minimumLogLevel))
		{
			builder = builder.WithMinimumLogLevel(minimumLogLevel);
		}

		if (configuration.GetValue<string?>("Index", null) is { } index)
		{
			builder = builder.WithIndex(index);
		}

		return builder;
	}
}