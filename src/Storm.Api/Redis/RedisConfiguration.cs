namespace Storm.Api.Redis;

public class RedisConfiguration
{
	/// <summary>
	/// List of Redis endpoints in the format "host:port"
	/// </summary>
	public required List<string> Endpoints { get; set; }

	/// <summary>
	/// User for Redis authentication
	/// </summary>
	public required string User { get; set; }

	/// <summary>
	/// Password for Redis authentication
	/// </summary>
	public required string Password { get; set; }

	/// <summary>
	/// Default database index to use
	/// </summary>
	public int DefaultDatabase { get; set; } = 0;

	/// <summary>
	/// Connection timeout in milliseconds
	/// </summary>
	public int ConnectTimeout { get; set; } = 5000;

	/// <summary>
	/// Number of times to retry connection
	/// </summary>
	public int ConnectRetry { get; set; } = 3;

	/// <summary>
	/// Client name for identification in Redis
	/// </summary>
	public string? ClientName { get; set; }

	/// <summary>
	/// Keep-alive interval in seconds
	/// </summary>
	public int KeepAliveSeconds { get; set; } = 60;

	/// <summary>
	/// Whether to abort connect if synchronization fails
	/// </summary>
	public bool AbortOnConnectFail { get; set; } = false;

	/// <summary>
	/// Channel prefix for pub/sub operations
	/// </summary>
	public string? ChannelPrefix { get; set; }
}