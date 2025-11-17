namespace Storm.Api.Redis;

public interface IRedisService
{
	Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null);
	Task<bool> SetAsync(string key, byte[] value, TimeSpan? expiry = null);
	Task<string?> GetStringAsync(string key);
	Task<byte[]?> GetBytesAsync(string key);
	Task<string?> GetAndDeleteAsync(string key);
	Task<bool> DeleteAsync(string key);
	Task<bool> PublishAsync(string channel, string message);
	Task<RedisSubscription> SubscribeAsync(string channel);
}