using Microsoft.Extensions.Configuration;
using Storm.Api.Core.Logs.ElasticSearch.Configurations;

namespace Storm.Api.Configurations
{
	public static class ElasticSearchConfigurationLoaderHelper
	{
		public static IElasticSearchConfigurationBuilder FromConfiguration(this IElasticSearchConfigurationBuilder builder, IConfiguration configuration)
		{
			/* Keys : Host ; User ; Password */

			return builder
				.WithNode(configuration["Host"])
				.WithBasicAuthentication(configuration["User"], configuration["Password"]);
		}
	}
}