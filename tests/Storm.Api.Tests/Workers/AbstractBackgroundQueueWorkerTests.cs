using Microsoft.Extensions.Logging;
using Storm.Api.Logs;
using Storm.Api.Logs.Interfaces;
using Storm.Api.Queues;
using Storm.Api.Workers.Queues;
using Storm.Api.Workers.Strategies;

namespace Storm.Api.Tests.Workers;

public class AbstractBackgroundQueueWorkerTests
{
	private sealed class NullSink : ILogSink
	{
		public void Enqueue(LogLevel level, string entry) { }
	}

	private static ILogService Logger() => new LogService(_ => new NullSink(), LogLevel.Trace);

	private sealed class TrackedRetry : IRetryStrategy
	{
		public bool Reset_Called;
		public bool Wait_Called;
		public int WaitCount;

		public bool DiscardAfterFailedAttempts { get; set; }
		public int AttemptsCountBeforeDiscard { get; set; } = 2;

		public void Reset() => Reset_Called = true;
		public Task Wait()
		{
			Wait_Called = true;
			WaitCount++;
			return Task.CompletedTask;
		}
	}

	private sealed class SimpleWorker : AbstractBackgroundQueueWorker<string, string>
	{
		public SimpleWorker(ILogService l, IItemQueue<string> q, Func<string, Task<bool>> action, IRetryStrategy? retry = null, Action<string, Exception>? onEx = null)
			: base(l, q, action, onEx, retry)
		{
		}
	}

	[Fact]
	public async Task Queue_FirstItem_StartsWorker()
	{
		ItemQueue<string> q = new();
		TaskCompletionSource<bool> done = new();
		SimpleWorker w = new(Logger(), q, _ =>
		{
			done.TrySetResult(true);
			return Task.FromResult(true);
		});
		w.Queue("hello");
		Task completed = await Task.WhenAny(done.Task, Task.Delay(1000));
		completed.Should().Be(done.Task);
	}

	[Fact]
	public async Task ProcessItemsAsync_Success_ResetsRetryStrategy()
	{
		ItemQueue<string> q = new();
		TrackedRetry retry = new();
		TaskCompletionSource<bool> done = new();
		SimpleWorker w = new(Logger(), q, _ =>
		{
			done.TrySetResult(true);
			return Task.FromResult(true);
		}, retry);
		w.Queue("x");
		await done.Task;
		await Task.Delay(50);
		retry.Reset_Called.Should().BeTrue();
	}

	[Fact]
	public async Task ProcessItemsAsync_TransientFailure_WaitsAndRetries()
	{
		ItemQueue<string> q = new();
		TrackedRetry retry = new();
		int calls = 0;
		TaskCompletionSource<bool> done = new();
		SimpleWorker w = new(Logger(), q, _ =>
		{
			calls++;
			if (calls >= 3)
			{
				done.TrySetResult(true);
				return Task.FromResult(true);
			}
			return Task.FromResult(false);
		}, retry);
		w.Queue("x");
		await done.Task;
		retry.WaitCount.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task ProcessItemsAsync_MaxAttemptsReached_DiscardsItem()
	{
		ItemQueue<string> q = new();
		TrackedRetry retry = new() { DiscardAfterFailedAttempts = true, AttemptsCountBeforeDiscard = 2 };
		int calls = 0;
		SimpleWorker w = new(Logger(), q, _ =>
		{
			calls++;
			return Task.FromResult(false);
		}, retry);
		w.Queue("x");
		await Task.Delay(200);
		calls.Should().BeGreaterThan(2);
	}

	[Fact]
	public async Task ProcessItemsAsync_OnItemsSuccess_CalledWithProcessedItems()
	{
		ItemQueue<string> q = new();
		TaskCompletionSource<string> done = new();
		SimpleWorker w = new(Logger(), q, x =>
		{
			done.TrySetResult(x);
			return Task.FromResult(true);
		});
		w.Queue("payload");
		string received = await done.Task;
		received.Should().Be("payload");
	}

	[Fact]
	public async Task ProcessItemsAsync_OnItemsError_CalledWithFailure()
	{
		ItemQueue<string> q = new();
		TrackedRetry retry = new() { DiscardAfterFailedAttempts = true, AttemptsCountBeforeDiscard = 1 };
		int calls = 0;
		SimpleWorker w = new(Logger(), q, _ =>
		{
			calls++;
			return Task.FromResult(false);
		}, retry);
		w.Queue("x");
		await Task.Delay(150);
		retry.Wait_Called.Should().BeTrue();
	}

	[Fact]
	public async Task ProcessItemsAsync_ExceptionOnUserAction_InvokesOnException()
	{
		ItemQueue<string> q = new();
		TrackedRetry retry = new() { DiscardAfterFailedAttempts = true, AttemptsCountBeforeDiscard = 0 };
		Exception? captured = null;
		SimpleWorker w = new(Logger(), q, _ => throw new InvalidOperationException("boom"), retry,
			onEx: (_, ex) => captured = ex);
		w.Queue("x");
		await Task.Delay(150);
		captured.Should().NotBeNull();
	}
}
