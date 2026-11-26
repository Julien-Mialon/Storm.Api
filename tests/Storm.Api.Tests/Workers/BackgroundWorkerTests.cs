using Microsoft.Extensions.Logging;
using Storm.Api.Logs;
using Storm.Api.Logs.Sinks.Consoles;
using Storm.Api.Workers;
using Xunit;

namespace Storm.Api.Tests.Workers;

public class BackgroundWorkerTests
{
	private static readonly TimeSpan STEP_TIMEOUT = TimeSpan.FromSeconds(10);

	[Fact]
	public async Task Start_before_the_loop_runs_does_not_start_another_loop()
	{
		int startedLoopsCount = 0;
		BackgroundWorker worker = new(SilentLogService(), async ct =>
		{
			Interlocked.Increment(ref startedLoopsCount);
			await Task.Delay(Timeout.Infinite, ct);
		});

		worker.Start();
		worker.Start();
		worker.Start();

		// Long enough for every loop started so far to run.
		await Task.Delay(TimeSpan.FromMilliseconds(200));
		worker.Stop();

		Assert.Equal(1, Volatile.Read(ref startedLoopsCount));
	}

	[Fact]
	public async Task Start_after_the_loop_ended_starts_a_new_loop()
	{
		int startedLoopsCount = 0;
		BackgroundWorker worker = new(SilentLogService(), async ct =>
		{
			// Only the first loop ends on its own.
			if (Interlocked.Increment(ref startedLoopsCount) > 1)
			{
				await Task.Delay(Timeout.Infinite, ct);
			}
		});

		worker.Start();

		using CancellationTokenSource timeout = new(STEP_TIMEOUT);
		while (Volatile.Read(ref startedLoopsCount) < 2)
		{
			await Task.Delay(TimeSpan.FromMilliseconds(10), timeout.Token);
			worker.Start();
		}

		worker.Start();
		await Task.Delay(TimeSpan.FromMilliseconds(200));
		worker.Stop();

		Assert.Equal(2, Volatile.Read(ref startedLoopsCount));
	}

	private static LogService SilentLogService()
	{
		return new(static _ => new ConsoleLogSink(), LogLevel.None);
	}
}
