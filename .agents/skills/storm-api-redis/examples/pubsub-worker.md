# Pub/Sub with a Background Worker

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
