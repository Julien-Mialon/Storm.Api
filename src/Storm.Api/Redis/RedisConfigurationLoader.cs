using Microsoft.Extensions.Configuration;

namespace Storm.Api.Redis;

public static class RedisConfigurationLoader
{
	public static RedisConfiguration LoadRedisConfiguration(this IConfiguration configuration)
	{
		return new RedisConfiguration
		{
			Endpoints = configuration.GetSection("Endpoints").Get<string[]>()?.ToList() ?? throw new ArgumentException("Missing Redis endpoints"),
			User = configuration.GetValue<string>("User") ?? throw new ArgumentException("Missing Redis user"),
			Password = configuration.GetValue<string>("Password") ?? throw new ArgumentException("Missing Redis password"),

			DefaultDatabase = configuration.GetValue<int>("DefaultDatabase", 0),
			ConnectTimeout = configuration.GetValue<int>("ConnectTimeout", 5000),
			ConnectRetry = configuration.GetValue<int>("ConnectRetry", 3),
			ClientName = configuration.GetValue<string>("ClientName"),
			KeepAliveSeconds = configuration.GetValue<int>("KeepAliveSeconds", 60),
			AbortOnConnectFail = configuration.GetValue<bool>("AbortOnConnectFail", false),
			ChannelPrefix = configuration.GetValue<string>("ChannelPrefix")
		};
	}
}