---
name: storm-api-queues
description: Implement in-memory queues using the Storm.Api framework with ItemQueue, BufferedItemQueue, ThrottledBufferedItemQueue, and queue workers.
user-invocable: true
disable-model-invocation: false
---

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

WorkItem item = await queue.Dequeue(cancellationToken);
```

### 2. BufferedItemQueue — Fixed-Size Batches

Collects items into arrays of up to `bufferSize`. `Dequeue` blocks until the buffer is full:

```csharp
var queue = new BufferedItemQueue<LogEntry>(bufferSize: 50);
LogEntry[] batch = await queue.Dequeue(cancellationToken);
```

### 3. ThrottledBufferedItemQueue — Batches with Timeout

Like `BufferedItemQueue`, but returns a partial batch if a timeout elapses:

```csharp
var queue = new ThrottledBufferedItemQueue<LogEntry>(
    bufferSize: 100,
    throttlingTime: TimeSpan.FromSeconds(5)
);
// Returns when 100 items collected OR 5 seconds after first item
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

## Choosing the Right Queue Type

| Scenario | Queue | Worker |
|----------|-------|--------|
| Process items immediately, one by one | `ItemQueue<T>` | `BackgroundQueueWorker<T>` or hosted `AbstractHostedServiceQueueWorker` |
| Batch items for bulk operations | `BufferedItemQueue<T>` | `BackgroundBufferedQueueWorker<T>` |
| Batch items but don't wait forever for a full batch | `ThrottledBufferedItemQueue<T>` | `BackgroundThrottledBufferedQueueWorker<T>` or hosted worker |
| Long-running worker managed by ASP.NET host | Any queue subclass | `AbstractHostedServiceQueueWorker` |
| Short-lived fire-and-forget within a service | `ItemQueue<T>` (internal) | `BackgroundQueueWorker<T>` (creates its own queue) |

---

## Using Queues with Workers

For standalone queue worker and hosted service queue worker examples, see [examples/queue-workers.md](examples/queue-workers.md).

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
