---
name: storm-api-workers
description: Implement background workers and hosted services using the Storm.Api framework with periodic, scheduled, and queue-based workers plus retry strategies.
user-invocable: true
disable-model-invocation: false
---

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
        var repo = services.GetRequiredService<ICleanupRepository>();
        await repo.DeleteExpiredSessions();
    }
}
```

Register: `services.AddHostedService<CleanupWorker>();`

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

Extend `AbstractHostedServiceQueueWorker<TInput, TOutput, TQueue>` to process items from a queue resolved from DI. For full queue worker examples, see [examples/queue-worker.md](examples/queue-worker.md).

---

## Multiple Instances & Standalone Workers

For running N parallel instances or fire-and-forget background tasks, see [examples/advanced-workers.md](examples/advanced-workers.md).

---

## Retry Strategies

Both queue workers and hosted service workers accept an optional `IRetryStrategy`. For strategy details and examples, see [examples/retry-strategies.md](examples/retry-strategies.md).

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
