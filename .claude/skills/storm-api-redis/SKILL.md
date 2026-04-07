---
name: storm-api-redis
description: Implement Redis caching and pub/sub using the Storm.Api framework with IRedisService for key-value storage, TTL, and channel subscriptions.
user-invocable: true
disable-model-invocation: false
---

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

Required: `Endpoints`, `User`, `Password`. All other properties are optional with sensible defaults. See `RedisConfiguration` class for full details.

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

```csharp
var redis = Resolve<IRedisService>();

// Set with TTL
await redis.SetAsync("user:123:name", "Alice", TimeSpan.FromMinutes(30));
await redis.SetAsync("user:123:avatar", bytes, TimeSpan.FromHours(1));

// Get
string? name = await redis.GetStringAsync("user:123:name");
byte[]? data = await redis.GetBytesAsync("user:123:avatar");

// Get and delete atomically (useful for one-time tokens)
string? token = await redis.GetAndDeleteAsync("reset-token:abc123");

// Delete
bool deleted = await redis.DeleteAsync("user:123:name");
```

---

## Pub/Sub

```csharp
// Publish
await redis.PublishAsync("events:user-created", userId.ToString());

// Subscribe (typically in a background worker)
await using var subscription = await redis.SubscribeAsync("events:user-created");
await foreach (string message in subscription.Read(cancellationToken))
{
    // Process each message
}
```

`RedisSubscription` implements `IAsyncDisposable` — always use `await using`. It automatically unsubscribes when enumeration ends or is cancelled.

---

## Full Examples

For a complete caching-in-action example, see [examples/caching.md](examples/caching.md).
For a pub/sub background worker example, see [examples/pubsub-worker.md](examples/pubsub-worker.md).

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
