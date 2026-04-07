# Retry Strategies

Both queue workers and hosted service workers accept an optional `IRetryStrategy`.

## Fixed Delay

```csharp
using Storm.Api.Workers.Strategies;

new DelayRetryStrategy(
    timeToWait: 5000,                  // 5 seconds between retries
    attemptsCountBeforeDiscard: 10     // give up after 10 failures (null = never discard)
);
```

## Exponential Backoff

```csharp
new ExponentialBackOffStrategy(
    baseMillisecondsCount: 2000,       // base delay unit
    maxIteration: 4,                   // caps exponential growth
    attemptsCountBeforeDiscard: 5      // give up after 5 failures (null = never discard)
);
// Delays: 0ms → 2s → 6s → 12s → 20s (capped at iteration 4)
```

The strategy resets on success and waits on failure. When `attemptsCountBeforeDiscard` is reached, the item is discarded.
