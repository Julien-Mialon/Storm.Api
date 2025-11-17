using StackExchange.Redis;
using Storm.Api.Extensions;

namespace Storm.Api.Redis;

public class RedisService : IRedisService, IAsyncDisposable
{
	private readonly SemaphoreSlim _connectionLock = new(1, 1);
	private readonly RedisConfiguration _configuration;
	private IConnectionMultiplexer? _connection;
	private ISubscriber? _subscriber;
	private bool _disposed;

	public RedisService(RedisConfiguration configuration)
	{
		_configuration = configuration;
	}

	private async Task<IConnectionMultiplexer> GetConnectionAsync()
	{
		if (_connection is { IsConnected: true })
		{
			return _connection;
		}

		using IDisposable semaphoreLock = await _connectionLock.Lock();
		if (_connection is { IsConnected: true })
		{
			return _connection;
		}

		ConfigurationOptions options = new()
		{
			User = _configuration.User,
			Password = _configuration.Password,
			DefaultDatabase = _configuration.DefaultDatabase,

			ConnectTimeout = _configuration.ConnectTimeout,
			SyncTimeout = _configuration.ConnectTimeout,
			AsyncTimeout = _configuration.ConnectTimeout * 2,

			AbortOnConnectFail = _configuration.AbortOnConnectFail,
			ConnectRetry = _configuration.ConnectRetry,
			KeepAlive = _configuration.KeepAliveSeconds,
		};

		if (_configuration.ClientName.IsNotNullOrEmpty())
		{
			options.ClientName = _configuration.ClientName;
		}

		if (_configuration.ChannelPrefix.IsNotNullOrEmpty())
		{
			options.ChannelPrefix = RedisChannel.Literal(_configuration.ChannelPrefix);
		}

		foreach (string endpoint in _configuration.Endpoints)
		{
			options.EndPoints.Add(endpoint);
		}

		_connection = await ConnectionMultiplexer.ConnectAsync(options);
		_subscriber = _connection.GetSubscriber();
		return _connection;
	}

	private async Task<ISubscriber> GetSubscriberAsync()
	{
		if (_subscriber != null)
		{
			return _subscriber;
		}

		await GetConnectionAsync();
		if (_subscriber == null)
		{
			throw new InvalidOperationException("Redis subscriber is not initialized");
		}

		return _subscriber;
	}

	public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
	{
		IConnectionMultiplexer connection = await GetConnectionAsync();
		IDatabase database = connection.GetDatabase();
		return await database.StringSetAsync(key, value, expiry);
	}

	public async Task<bool> SetAsync(string key, byte[] value, TimeSpan? expiry = null)
	{
		IConnectionMultiplexer connection = await GetConnectionAsync();
		IDatabase database = connection.GetDatabase();
		return await database.StringSetAsync(key, value, expiry);
	}

	public async Task<string?> GetStringAsync(string key)
	{
		IConnectionMultiplexer connection = await GetConnectionAsync();
		IDatabase database = connection.GetDatabase();
		RedisValue value = await database.StringGetAsync(key);
		return value.HasValue ? value.ToString() : null;
	}

	public async Task<byte[]?> GetBytesAsync(string key)
	{
		IConnectionMultiplexer connection = await GetConnectionAsync();
		IDatabase database = connection.GetDatabase();
		RedisValue value = await database.StringGetAsync(key);
		return value.HasValue ? (byte[]?)value : null;
	}

	public async Task<string?> GetAndDeleteAsync(string key)
	{
		IConnectionMultiplexer connection = await GetConnectionAsync();
		IDatabase database = connection.GetDatabase();
		RedisValue value = await database.StringGetDeleteAsync(key);
		return value.HasValue ? value.ToString() : null;
	}

	public async Task<bool> DeleteAsync(string key)
	{
		IConnectionMultiplexer connection = await GetConnectionAsync();
		IDatabase database = connection.GetDatabase();
		return await database.KeyDeleteAsync(key);
	}

	public async Task<bool> PublishAsync(string channel, string message)
	{
		ISubscriber subscriber = await GetSubscriberAsync();
		long subscribers = await subscriber.PublishAsync(RedisChannel.Literal(channel), message);
		return subscribers > 0;
	}

	public async Task<RedisSubscription> SubscribeAsync(string channel)
	{
		ISubscriber subscriber = await GetSubscriberAsync();

		ChannelMessageQueue queue = await subscriber.SubscribeAsync(new RedisChannel(channel, RedisChannel.PatternMode.Auto));
		RedisSubscription subscription = new(queue);
		return subscription;
	}

	public async ValueTask DisposeAsync()
	{
		if (_disposed)
		{
			return;
		}

		if (_connection != null)
		{
			await _connection.DisposeAsync();
		}

		_connectionLock.Dispose();
		_disposed = true;
		GC.SuppressFinalize(this);
	}
}