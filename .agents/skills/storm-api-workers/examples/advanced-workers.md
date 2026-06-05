# Advanced Workers

## Multiple Instances

Run N parallel instances of any hosted service:

```csharp
using Storm.Api.Workers.HostedServices;

services.AddSingleton<MultipleInstanceHostedService<NotificationWorker>>(sp =>
    new MultipleInstanceHostedService<NotificationWorker>(sp, count: 3));
services.AddHostedService(sp =>
    sp.GetRequiredService<MultipleInstanceHostedService<NotificationWorker>>());
```

Or with a custom factory:
```csharp
new MultipleInstanceHostedService<NotificationWorker>(sp, count: 3,
    factory: provider => new NotificationWorker(provider));
```
