You are helping implement in-memory queues using the **Storm.Api** framework. Follow all patterns below exactly. For global rules (logging, extensions, anti-patterns), see `/storm-api`.

The user's request: $ARGUMENTS

---

## How Queues Work

Storm.Api provides channel-based in-memory queues for decoupling producers from consumers. Queues are used with background workers (see `/storm-api-workers`) to process items asynchronously. Three queue types are available, all built on `System.Threading.Channels`.

---

## Queue Types

### 1. ItemQueue — One-at-a-Time

Unbounded queue where each `Dequeue` returns a single item:

```csharp
using Storm.Api.Queues;

var queue = new ItemQueue<WorkItem>();
queue.Queue(new WorkItem { ... });

// Consumer (blocks until item available)
WorkItem item = await queue.Dequeue(cancellationToken);

// Or stream all items
await foreach (var item in queue.DequeueAll(cancellationToken))
{
    await Process(item);
}
```

### 2. BufferedItemQueue — Fixed-Size Batches

Collects items into arrays of up to `bufferSize`. `Dequeue` blocks until the buffer is full:

```csharp
using Storm.Api.Queues;

var queue = new BufferedItemQueue<LogEntry>(bufferSize: 50);
queue.Queue(new LogEntry { ... });

// Returns up to 50 items at once
LogEntry[] batch = await queue.Dequeue(cancellationToken);
await BulkInsert(batch);
```

### 3. ThrottledBufferedItemQueue — Batches with Timeout

Like `BufferedItemQueue`, but returns a partial batch if a timeout elapses. Waits indefinitely for the first item, then collects more items until the buffer is full or `throttlingTime` expires:

```csharp
using Storm.Api.Queues;

var queue = new ThrottledBufferedItemQueue<LogEntry>(
    bufferSize: 100,
    throttlingTime: TimeSpan.FromSeconds(5)
);

// Returns when 100 items collected OR 5 seconds after first item, whichever comes first
LogEntry[] batch = await queue.Dequeue(cancellationToken);
```

---

## Interfaces

| Interface | Input → Output | Use case |
|-----------|---------------|----------|
| `IItemQueue<TInput, TOutput>` | `TInput` → `TOutput` | Base interface — `Queue(TInput)`, `Dequeue() → TOutput` |
| `IItemQueue<TWorkItem>` | `T` → `T` | Shorthand where input and output are the same type |
| `IBufferedItemQueue<TWorkItem>` | `T` → `T[]` | Specialization for batched output |

---

## Using Queues with Background Workers

Queues are typically paired with queue workers. There are two approaches:

### Approach 1 — Standalone Queue Worker (IWorker)

Workers that manage their own lifecycle. Useful for fire-and-forget processing within a running application:

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

### Approach 2 — Hosted Service Queue Worker

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

---

## Choosing the Right Queue Type

| Scenario | Queue | Worker |
|----------|-------|--------|
| Process items immediately, one by one | `ItemQueue<T>` | `BackgroundQueueWorker<T>` or hosted `AbstractHostedServiceQueueWorker` |
| Batch items for bulk operations (e.g., bulk DB insert) | `BufferedItemQueue<T>` | `BackgroundBufferedQueueWorker<T>` |
| Batch items but don't wait forever for a full batch | `ThrottledBufferedItemQueue<T>` | `BackgroundThrottledBufferedQueueWorker<T>` or hosted worker |
| Long-running worker managed by ASP.NET host | Any queue subclass | `AbstractHostedServiceQueueWorker` |
| Short-lived fire-and-forget within a service | `ItemQueue<T>` (internal) | `BackgroundQueueWorker<T>` (creates its own queue) |

---

## When NOT to Use In-Memory Queues

- If queued items must survive app restarts or crashes, use Redis pub/sub (`/storm-api-redis`) or an external message broker instead — in-memory queues are lost on shutdown.
- If multiple app instances need to share a queue, use Redis or an external broker — in-memory queues are per-process.

---

## Anti-Patterns to Avoid

| ❌ Wrong | ✅ Correct |
|---|---|
| Use `ConcurrentQueue<T>` or `BlockingCollection<T>` | Use `ItemQueue<T>` (channel-based, async-native) |
| Poll the queue in a `while` loop with `Task.Delay` | Use `Dequeue()` which blocks efficiently via channels |
| Create queue workers without retry strategies for unreliable operations | Use `DelayRetryStrategy` or `ExponentialBackOffStrategy` |
| Register queue as transient/scoped | Register as **singleton** — producers and consumers must share the same instance |
