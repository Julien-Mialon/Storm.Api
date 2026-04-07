# Queue Workers

## Approach 1 — Standalone Queue Worker (IWorker)

Workers that manage their own lifecycle. Useful for fire-and-forget processing:

```csharp
using Storm.Api.Workers.Queues;
using Storm.Api.Workers.Strategies;

// One-at-a-time processing
var worker = new BackgroundQueueWorker<WorkItem>(
    logService,
    itemAction: async item => { await Process(item); return true; },
    onException: null,
    retryStrategy: new DelayRetryStrategy(timeToWait: 3000, attemptsCountBeforeDiscard: 5)
);
worker.Queue(new WorkItem { ... }); // starts worker automatically on first queue

// Batched processing
var batchWorker = new BackgroundBufferedQueueWorker<LogEntry>(
    logService,
    itemAction: async batch => { await BulkInsert(batch); return true; },
    onException: null,
    bufferSize: 50,
    retryStrategy: new ExponentialBackOffStrategy(2000, 4, 3)
);

// Batched with timeout
var throttledWorker = new BackgroundThrottledBufferedQueueWorker<LogEntry>(
    logService,
    itemAction: async batch => { await BulkInsert(batch); return true; },
    throttlingTime: TimeSpan.FromSeconds(5),
    onException: null,
    bufferSize: 100,
    retryStrategy: null
);
```

## Approach 2 — Hosted Service Queue Worker

For long-running workers managed by the ASP.NET Core host. The queue is resolved from DI:

```csharp
using Storm.Api.Queues;
using Storm.Api.Workers.HostedServices;
using Storm.Api.Workers.Strategies;

// 1. Define a named queue (subclass for DI registration)
public class EmailQueue : ItemQueue<EmailMessage>;

// 2. Implement the hosted worker
public class EmailWorker(IServiceProvider services)
    : AbstractHostedServiceQueueWorker<EmailMessage, EmailMessage, EmailQueue>(
        services,
        new ExponentialBackOffStrategy(2000, 4, 5))
{
    protected override async Task<bool> DoAction(EmailMessage item)
    {
        var emailService = Resolve<IEmailService>();
        await emailService.Send(item.ToEmailContent());
        return true; // true = success, false = retry
    }
}

// 3. Register both
services.AddSingleton<EmailQueue>();
services.AddHostedService<EmailWorker>();

// 4. Enqueue from any action
var queue = Resolve<EmailQueue>();
queue.Queue(new EmailMessage { ... });
```

## Buffered Hosted Worker

For buffered hosted workers, define a `ThrottledBufferedItemQueue` subclass:

```csharp
public class LogQueue : ThrottledBufferedItemQueue<string>
{
    public LogQueue() : base(bufferSize: 100, throttlingTime: TimeSpan.FromSeconds(10)) { }
}

public class LogWorker(IServiceProvider services)
    : AbstractHostedServiceQueueWorker<string, string[], LogQueue>(
        services,
        new ExponentialBackOffStrategy(5000, 4, 3))
{
    protected override async Task<bool> DoAction(string[] batch)
    {
        var sender = Resolve<IBulkLogSender>();
        return await sender.Send(batch);
    }
}
```
