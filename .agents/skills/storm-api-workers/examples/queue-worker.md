# Queue Worker (Hosted Service)

Extend `AbstractHostedServiceQueueWorker<TInput, TOutput, TQueue>` to process items from a queue resolved from DI:

```csharp
using Storm.Api.Workers.HostedServices;
using Storm.Api.Workers.Strategies;
using Storm.Api.Queues;

// 1. Define a queue (register as singleton)
public class NotificationQueue : ItemQueue<NotificationMessage>;

// 2. Implement the worker
public class NotificationWorker(IServiceProvider services)
    : AbstractHostedServiceQueueWorker<NotificationMessage, NotificationMessage, NotificationQueue>(
        services,
        new ExponentialBackOffStrategy(baseMillisecondsCount: 2000, maxIteration: 4, attemptsCountBeforeDiscard: 5))
{
    protected override async Task<bool> DoAction(NotificationMessage item)
    {
        var sender = Resolve<INotificationSender>();
        await sender.Send(item);
        return true; // true = success, false = retry
    }
}

// 3. Register
services.AddSingleton<NotificationQueue>();
services.AddHostedService<NotificationWorker>();
```

To enqueue items from an action:
```csharp
var queue = Resolve<NotificationQueue>();
queue.Queue(new NotificationMessage { ... });
```
