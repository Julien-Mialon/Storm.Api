You are helping implement Redis caching and pub/sub using the **Storm.Api** framework. Follow all patterns below exactly. For global rules (logging, extensions, anti-patterns), see `/storm-api`.

The user's request: $ARGUMENTS

---

## How Redis Works

Storm.Api wraps `StackExchange.Redis` behind an `IRedisService` interface providing key-value storage with TTL and pub/sub messaging. The connection is lazy (created on first use) and thread-safe.

---

## Configuration

### appsettings.json

```json
{
  "Redis": {
    "Endpoints": ["localhost:6379"],
    "User": "default",
    "Password": "your-password",
    "DefaultDatabase": 0,
    "ConnectTimeout": 5000,
    "ConnectRetry": 3,
    "ClientName": "my-app",
    "KeepAliveSeconds": 60,
    "AbortOnConnectFail": false,
    "ChannelPrefix": "myapp:"
  }
}
```

Required properties: `Endpoints` (list of `host:port`), `User`, `Password`. Optional: `DefaultDatabase` (default `0`), `ConnectTimeout` (default `5000`ms), `ConnectRetry` (default `3`), `ClientName`, `KeepAliveSeconds` (default `60`), `AbortOnConnectFail` (default `false`), `ChannelPrefix` (prefix for pub/sub channels). See `RedisConfiguration` class for full details.

### Register in Startup.ConfigureServices

```csharp
using Storm.Api.Redis;

var redisConfig = configuration.GetSection("Redis").LoadRedisConfiguration();
services.AddSingleton(redisConfig);
services.AddSingleton<IRedisService, RedisService>();
```

---

## Key-Value Operations

Use `IRedisService` inside any action or service via `Resolve<IRedisService>()`:

### Set a value

```csharp
var redis = Resolve<IRedisService>();

// String value with TTL
await redis.SetAsync("user:123:name", "Alice", TimeSpan.FromMinutes(30));

// Binary value
byte[] data = Serialize(myObject);
await redis.SetAsync("user:123:avatar", data, TimeSpan.FromHours(1));

// No expiry (persists until deleted)
await redis.SetAsync("config:feature-x", "enabled");
```

### Get a value

```csharp
string? name = await redis.GetStringAsync("user:123:name");  // null if key missing
byte[]? avatar = await redis.GetBytesAsync("user:123:avatar");
```

### Get and delete atomically

```csharp
// Retrieve value and remove it in one operation (useful for one-time tokens)
string? token = await redis.GetAndDeleteAsync("reset-token:abc123");
```

### Delete a key

```csharp
bool deleted = await redis.DeleteAsync("user:123:name"); // true if key existed
```

---

## Pub/Sub

### Publish a message

```csharp
var redis = Resolve<IRedisService>();
await redis.PublishAsync("events:user-created", userId.ToString());
```

### Subscribe to a channel

Subscriptions return an `IAsyncEnumerable<string>` — typically consumed in a background worker:

```csharp
var redis = Resolve<IRedisService>();
RedisSubscription subscription = await redis.SubscribeAsync("events:user-created");

await foreach (string message in subscription.Read(cancellationToken))
{
    // Process each message
    var userId = Guid.Parse(message);
    await HandleUserCreated(userId);
}
// Automatically unsubscribes when enumeration ends or is cancelled
```

`RedisSubscription` implements both `IDisposable` and `IAsyncDisposable` — prefer `await using` when used in a scoped context:

```csharp
await using var subscription = await redis.SubscribeAsync("my-channel");
await foreach (string msg in subscription.Read(ct))
{
    // ...
}
```

---

## Full Example: Caching in an Action

```csharp
using Storm.Api.CQRS;
using Storm.Api.Redis;

public class GetUserProfileQuery(IServiceProvider services)
    : BaseAction<GetUserProfileParameter, UserProfileDto>(services)
{
    protected override async Task<UserProfileDto> Action(GetUserProfileParameter parameter)
    {
        var redis = Resolve<IRedisService>();
        string cacheKey = $"profile:{parameter.UserId}";

        string? cached = await redis.GetStringAsync(cacheKey);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<UserProfileDto>(cached)!;
        }

        var repo = Resolve<IUserRepository>();
        var user = await repo.GetById(parameter.UserId)
            ?? throw new DomainException("USER_NOT_FOUND", "User not found");

        var dto = new UserProfileDto { Id = user.Id, Name = user.Name };
        await redis.SetAsync(cacheKey, JsonSerializer.Serialize(dto), TimeSpan.FromMinutes(10));
        return dto;
    }
}
```

---

## Full Example: Pub/Sub with a Background Worker

```csharp
using Storm.Api.Workers.HostedServices;
using Storm.Api.Redis;

public class UserEventListener(IServiceProvider services)
    : BaseHostedService(services)
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var redis = Resolve<IRedisService>();
        await using var subscription = await redis.SubscribeAsync("events:user-created");

        await foreach (string message in subscription.Read(stoppingToken))
        {
            await Services.ExecuteWithScope(async sp =>
            {
                var emailService = sp.GetRequiredService<IEmailService>();
                // send welcome email...
            });
        }
    }
}
```

Register:
```csharp
services.AddHostedService<UserEventListener>();
```

---

## When NOT to Use Redis

- For simple in-process decoupling (producer/consumer within one app instance), prefer in-memory queues (`/storm-api-queues`) — no external dependency needed.
- For data that must be strongly consistent or transactional, use the database directly.

---

## Anti-Patterns to Avoid

| ❌ Wrong | ✅ Correct |
|---|---|
| Create `ConnectionMultiplexer` manually | Register `IRedisService` as singleton via DI |
| Use `StackExchange.Redis` types directly in actions | Use `IRedisService` abstraction |
| Subscribe in an action (short-lived scope) | Subscribe in a hosted service (long-lived) |
| Store objects without serialization | Serialize to string/bytes before storing |
