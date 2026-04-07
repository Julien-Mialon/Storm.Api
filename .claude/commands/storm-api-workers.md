You are helping implement background workers and hosted services using the **Storm.Api** framework. Follow all patterns below exactly. For global rules (logging, extensions, anti-patterns), see `/storm-api`.

The user's request: $ARGUMENTS

---

## How Background Workers Work

Storm.Api provides base classes for running background tasks as ASP.NET Core hosted services. All extend `BaseHostedService` (which extends `BackgroundService`) and use `Resolve<T>()` for DI.

---

## Option 1 — Periodic Task (Fixed Interval)

Extend `BasePeriodicRunHostedService` to run a task at a fixed interval. Each run gets its own DI scope.

```csharp
using Storm.Api.Workers.HostedServices;

public class CleanupWorker(IServiceProvider services)
    : BasePeriodicRunHostedService(services, TimeSpan.FromMinutes(30))
{
    protected override async Task Run(IServiceProvider services)
    {
        // services is a scoped provider — resolve scoped services from it
        var repo = services.GetRequiredService<ICleanupRepository>();
        await repo.DeleteExpiredSessions();
    }

    // Optional: override to customize error handling (default logs as Critical)
    protected override void OnException(Exception ex)
    {
        base.OnException(ex);
    }
}
```

Register as a hosted service:
```csharp
services.AddHostedService<CleanupWorker>();
```

---

## Option 2 — Scheduled Task (Specific Times of Day)

Extend `BaseTimeRunHostedService` to run at specific times each day:

```csharp
using Storm.Api.Workers.HostedServices;

public class DailyReportWorker(IServiceProvider services)
    : BaseTimeRunHostedService(services,
        new TimeOnly(8, 0),   // 08:00
        new TimeOnly(20, 0))  // 20:00
{
    protected override async Task Run(IServiceProvider services)
    {
        var reportService = services.GetRequiredService<IReportService>();
        await reportService.GenerateDailyReport();
    }
}
```

---

## Option 3 — Queue Worker (Hosted Service)

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

---

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

---

## Standalone BackgroundWorker

For fire-and-forget background tasks outside the hosted service model:

```csharp
using Storm.Api.Workers;

var worker = new BackgroundWorker(logService, async ct =>
{
    while (!ct.IsCancellationRequested)
    {
        await DoWork(ct);
    }
});
worker.Start();
// later...
worker.Stop();
```

Thread-safe — `Start()` and `Stop()` can be called from any thread.

---

## Retry Strategies

Both queue workers and hosted service workers accept an optional `IRetryStrategy`:

### Fixed Delay

```csharp
using Storm.Api.Workers.Strategies;

new DelayRetryStrategy(
    timeToWait: 5000,                  // 5 seconds between retries
    attemptsCountBeforeDiscard: 10     // give up after 10 failures (null = never discard)
);
```

### Exponential Backoff

```csharp
new ExponentialBackOffStrategy(
    baseMillisecondsCount: 2000,       // base delay unit
    maxIteration: 4,                   // caps exponential growth
    attemptsCountBeforeDiscard: 5      // give up after 5 failures (null = never discard)
);
// Delays: 0ms → 2s → 6s → 12s → 20s (capped at iteration 4)
```

The strategy resets on success and waits on failure. When `attemptsCountBeforeDiscard` is reached, the item is discarded.

---

## Built-in: App Metrics

Storm.Api includes `AppMetricsHostedService` that logs thread pool and GC stats every 5 seconds:

```csharp
services.AddHostedService<AppMetricsHostedService>();
```

---

## When NOT to Use In-Process Workers

- If you need work to survive app restarts, use an external queue (e.g., Redis pub/sub via `/storm-api-redis`) instead of in-memory queues.
- If the task is a simple one-off async call within a request, just `await` it in the action — don't over-engineer with a hosted service.

---

## Anti-Patterns to Avoid

| ❌ Wrong | ✅ Correct |
|---|---|
| Use `Task.Run()` for background work | Use a hosted service or `BackgroundWorker` |
| Resolve scoped services from the constructor | Use the `services` parameter in `Run()`, or `Resolve<T>()` in workers |
| Retry manually with `Thread.Sleep` | Use `IRetryStrategy` |
