using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Storm.Api.Logs;
using Storm.Api.Logs.Interfaces;
using Storm.Api.Workers.Strategies;
using Xunit;

namespace Storm.Api.Tests.Workers;

/// <summary>
/// The retry loop shared by the queue workers; each derived class runs these tests on one of them. The
/// retry strategy hands every wait to the test, so a worker may only retry once the test releases it.
/// </summary>
public abstract class QueueWorkerRetryTests
{
	private const int ATTEMPTS_COUNT_BEFORE_DISCARD = 2;
	private const int ATTEMPTS_COUNT = ATTEMPTS_COUNT_BEFORE_DISCARD + 1;

	private static readonly TimeSpan STEP_TIMEOUT = TimeSpan.FromSeconds(10);

	private readonly ConcurrentQueue<string> _logEntries = new();

	protected ILogService LogService { get; }

	protected QueueWorkerRetryTests()
	{
		LogService = new LogService(_ => new RecordingLogSink(_logEntries), LogLevel.Trace);
	}

	/// <summary>Starts the worker under test, which hands every item it dequeues to <paramref name="action"/>.</summary>
	/// <returns>Queues an item on that worker.</returns>
	protected abstract Task<Action<string>> Start(Func<string, Task<bool>> action, IRetryStrategy retryStrategy);

	[Fact]
	public async Task Failed_attempt_waits_for_the_retry_strategy_before_retrying()
	{
		RecordedAction action = new(static (item, attempt) => item != "first" || attempt > 1);
		ManualRetryStrategy retryStrategy = new(discardAfterFailedAttempts: true);
		Action<string> queue = await Start(action.Invoke, retryStrategy);
		queue("first");
		queue("next");

		TaskCompletionSource firstWait = await Next(retryStrategy.Waits);

		// Long enough for a worker that does not await the wait to retry right away.
		await Task.Delay(TimeSpan.FromMilliseconds(200));
		Assert.Equal(["first"], action.Attempts);

		firstWait.SetResult();

		Assert.Equal("first", await Next(action.Succeeded));
		Assert.Equal("next", await Next(action.Succeeded));
		Assert.Equal(["first", "first", "next"], action.Attempts);
		Assert.Equal(0, retryStrategy.Waits.Count);
	}

	[Fact]
	public async Task Item_that_keeps_returning_false_is_discarded_after_its_last_attempt()
	{
		await AssertDiscarded(new RecordedAction(static (item, _) => item != "first"));
	}

	[Fact]
	public async Task Item_that_keeps_throwing_is_discarded_after_its_last_attempt()
	{
		await AssertDiscarded(new RecordedAction(static (item, _) => item != "first" ? true : throw new InvalidOperationException("Simulated failure")));
	}

	[Fact]
	public async Task Item_is_retried_until_it_succeeds_when_the_strategy_never_discards()
	{
		const int FAILED_ATTEMPTS_COUNT = ATTEMPTS_COUNT + 1;
		RecordedAction action = new(static (_, attempt) => attempt > FAILED_ATTEMPTS_COUNT);
		ManualRetryStrategy retryStrategy = new(discardAfterFailedAttempts: false);
		Action<string> queue = await Start(action.Invoke, retryStrategy);
		queue("first");

		await ReleaseWaits(retryStrategy, FAILED_ATTEMPTS_COUNT);

		Assert.Equal("first", await Next(action.Succeeded));
		Assert.Equal(FAILED_ATTEMPTS_COUNT + 1, action.Attempts.Count);
		Assert.DoesNotContain(_logEntries, static entry => entry.Contains("Discarding item"));
	}

	/// <summary>Queues "first", which <paramref name="action"/> fails on every attempt, then "next".</summary>
	private async Task AssertDiscarded(RecordedAction action)
	{
		ManualRetryStrategy retryStrategy = new(discardAfterFailedAttempts: true);
		Action<string> queue = await Start(action.Invoke, retryStrategy);
		queue("first");
		queue("next");

		// No wait after the last attempt: the item is discarded right away.
		await ReleaseWaits(retryStrategy, ATTEMPTS_COUNT - 1);

		Assert.Equal("next", await Next(action.Succeeded));
		Assert.Equal([.. Enumerable.Repeat("first", ATTEMPTS_COUNT), "next"], action.Attempts);
		Assert.Equal(0, retryStrategy.Waits.Count);
		Assert.Single(_logEntries, static entry => entry.Contains($"Discarding item after {ATTEMPTS_COUNT} failed attempts"));
	}

	private static async Task ReleaseWaits(ManualRetryStrategy retryStrategy, int count)
	{
		for (int wait = 0 ; wait < count ; wait++)
		{
			(await Next(retryStrategy.Waits)).SetResult();
		}
	}

	private static async Task<T> Next<T>(ChannelReader<T> reader)
	{
		using CancellationTokenSource timeout = new(STEP_TIMEOUT);
		return await reader.ReadAsync(timeout.Token);
	}

	/// <summary>Records every attempt in order; the outcome gets the item and its attempt number, starting at 1.</summary>
	private sealed class RecordedAction(Func<string, int, bool> outcome)
	{
		private readonly ConcurrentDictionary<string, int> _attemptsPerItem = new();
		private readonly Channel<string> _succeeded = Channel.CreateUnbounded<string>();

		public ConcurrentQueue<string> Attempts { get; } = new();

		public ChannelReader<string> Succeeded => _succeeded.Reader;

		public Task<bool> Invoke(string item)
		{
			Attempts.Enqueue(item);
			int attempt = _attemptsPerItem.AddOrUpdate(item, 1, static (_, count) => count + 1);
			if (outcome(item, attempt) is false)
			{
				return Task.FromResult(false);
			}

			_succeeded.Writer.TryWrite(item);
			return Task.FromResult(true);
		}
	}

	private sealed class ManualRetryStrategy(bool discardAfterFailedAttempts) : IRetryStrategy
	{
		private readonly Channel<TaskCompletionSource> _waits = Channel.CreateUnbounded<TaskCompletionSource>();

		public bool DiscardAfterFailedAttempts { get; } = discardAfterFailedAttempts;

		public int AttemptsCountBeforeDiscard => ATTEMPTS_COUNT_BEFORE_DISCARD;

		public ChannelReader<TaskCompletionSource> Waits => _waits.Reader;

		public void Reset()
		{
		}

		public Task Wait()
		{
			TaskCompletionSource wait = new(TaskCreationOptions.RunContinuationsAsynchronously);
			_waits.Writer.TryWrite(wait);
			return wait.Task;
		}
	}

	private sealed class RecordingLogSink(ConcurrentQueue<string> entries) : ILogSink
	{
		public void Enqueue(LogLevel level, string entry)
		{
			entries.Enqueue(entry);
		}
	}
}
