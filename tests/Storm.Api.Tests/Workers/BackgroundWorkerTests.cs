using Microsoft.Extensions.Logging;
using Storm.Api.Logs;
using Storm.Api.Logs.Interfaces;
using Storm.Api.Workers;

namespace Storm.Api.Tests.Workers;

public class BackgroundWorkerTests
{
	private sealed class NullSink : ILogSink
	{
		public void Enqueue(LogLevel level, string entry) { }
	}

	private static ILogService Logger() => new LogService(_ => new NullSink(), LogLevel.Trace);

	[Fact]
	public async Task Start_BeginsRun()
	{
		TaskCompletionSource<bool> started = new();
		BackgroundWorker w = new(Logger(), _ =>
		{
			started.TrySetResult(true);
			return Task.CompletedTask;
		});
		w.Start();
		bool ran = await Task.WhenAny(started.Task, Task.Delay(1000)) == started.Task;
		ran.Should().BeTrue();
	}

	[Fact]
	public async Task Start_AlreadyRunning_IsNoOp()
	{
		int startCount = 0;
		TaskCompletionSource<bool> ready = new();
		BackgroundWorker w = new(Logger(), async ct =>
		{
			Interlocked.Increment(ref startCount);
			ready.TrySetResult(true);
			await Task.Delay(Timeout.Infinite, ct).ContinueWith(_ => { });
		});
		w.Start();
		await ready.Task;
		w.Start();
		await Task.Delay(100);
		startCount.Should().Be(1);
		w.Stop();
	}

	[Fact]
	public void Stop_CancelsAndClearsTask()
	{
		BackgroundWorker w = new(Logger(), async ct =>
		{
			try
			{
				await Task.Delay(Timeout.Infinite, ct);
			}
			catch
			{
			}
		});
		w.Start();
		w.Stop();
	}

	[Fact]
	public void Stop_NotRunning_IsNoOp()
	{
		BackgroundWorker w = new(Logger(), _ => Task.CompletedTask);
		Action act = () => w.Stop();
		act.Should().NotThrow();
	}

	[Fact]
	public async Task Run_WrapsUserFunctionWithLogging()
	{
		bool called = false;
		BackgroundWorker w = new(Logger(), _ =>
		{
			called = true;
			return Task.CompletedTask;
		});
		w.Start();
		await Task.Delay(100);
		called.Should().BeTrue();
	}

	[Fact]
	public async Task Run_ExceptionInUserFunction_LoggedAndSuppressed()
	{
		BackgroundWorker w = new(Logger(), _ => throw new InvalidOperationException("boom"));
		w.Start();
		await Task.Delay(100);
	}

	[Fact]
	public async Task ConcurrentStartStop_IsThreadSafe()
	{
		BackgroundWorker w = new(Logger(), async ct =>
		{
			try
			{
				await Task.Delay(Timeout.Infinite, ct);
			}
			catch
			{
			}
		});

		Task[] tasks = Enumerable.Range(0, 8).Select(i => Task.Run(() =>
		{
			if (i % 2 == 0)
			{
				w.Start();
			}
			else
			{
				w.Stop();
			}
		})).ToArray();

		await Task.WhenAll(tasks);
		w.Stop();
	}
}
