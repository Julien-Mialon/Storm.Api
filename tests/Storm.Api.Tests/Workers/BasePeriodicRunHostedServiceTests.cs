using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Storm.Api.Logs;
using Storm.Api.Logs.Interfaces;
using Storm.Api.Workers.HostedServices;

namespace Storm.Api.Tests.Workers;

public class BasePeriodicRunHostedServiceTests
{
	private sealed class NullSink : ILogSink
	{
		public void Enqueue(LogLevel level, string entry) { }
	}

	private sealed class CountingService(IServiceProvider services, TimeSpan interval, Func<IServiceProvider, int, Task>? body = null)
		: BasePeriodicRunHostedService(services, interval)
	{
		public int Iterations;
		public int Exceptions;
		private readonly Func<IServiceProvider, int, Task> _body = body ?? ((_, _) => Task.CompletedTask);

		protected override async Task Run(IServiceProvider services)
		{
			int iteration = Interlocked.Increment(ref Iterations);
			await _body(services, iteration);
		}

		protected override void OnException(Exception ex) => Interlocked.Increment(ref Exceptions);

		public Task StartFor(CancellationToken ct) => StartAsync(ct);
		public Task StopFor(CancellationToken ct) => StopAsync(ct);
	}

	private static IServiceProvider Provider()
	{
		ServiceCollection sc = new();
		sc.AddSingleton<ILogService>(new LogService(_ => new NullSink(), LogLevel.Trace));
		return sc.BuildServiceProvider();
	}

	private static readonly TimeSpan TEST_TIMEOUT = TimeSpan.FromSeconds(5);

	[Fact]
	public async Task ExecuteAsync_CallsRunRepeatedly_RespectingInterval()
	{
		TaskCompletionSource secondIteration = new();
		CountingService s = new(Provider(), TimeSpan.FromMilliseconds(1), (_, n) =>
		{
			if (n >= 2)
			{
				secondIteration.TrySetResult();
			}

			return Task.CompletedTask;
		});
		using CancellationTokenSource cts = new();
		_ = s.StartFor(cts.Token);

		await secondIteration.Task.WaitAsync(TEST_TIMEOUT);
		cts.Cancel();

		s.Iterations.Should().BeGreaterThanOrEqualTo(2);
	}

	[Fact]
	public async Task ExecuteAsync_CreatesAndDisposesScopePerIteration()
	{
		TaskCompletionSource firstIteration = new();
		int scopesSeen = 0;
		CountingService s = new(Provider(), TimeSpan.FromMilliseconds(1), (sp, _) =>
		{
			sp.Should().NotBeNull();
			Interlocked.Increment(ref scopesSeen);
			firstIteration.TrySetResult();
			return Task.CompletedTask;
		});
		using CancellationTokenSource cts = new();
		_ = s.StartFor(cts.Token);

		await firstIteration.Task.WaitAsync(TEST_TIMEOUT);
		cts.Cancel();

		scopesSeen.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task ExecuteAsync_CancellationStopsLoop()
	{
		TaskCompletionSource firstIteration = new();
		CountingService s = new(Provider(), TimeSpan.FromMilliseconds(20), (_, _) =>
		{
			firstIteration.TrySetResult();
			return Task.CompletedTask;
		});
		using CancellationTokenSource cts = new();
		Task loop = s.StartFor(cts.Token);

		await firstIteration.Task.WaitAsync(TEST_TIMEOUT);
		cts.Cancel();
		await s.StopFor(CancellationToken.None);

		int afterCancel = s.Iterations;
		await Task.Delay(100);
		s.Iterations.Should().Be(afterCancel);
	}

	[Fact]
	public async Task ExecuteAsync_ExceptionInRun_LoggedAndLoopContinues()
	{
		TaskCompletionSource secondIteration = new();
		CountingService s = new(Provider(), TimeSpan.FromMilliseconds(1), (_, n) =>
		{
			if (n == 1)
			{
				throw new InvalidOperationException("boom");
			}

			if (n >= 2)
			{
				secondIteration.TrySetResult();
			}

			return Task.CompletedTask;
		});
		using CancellationTokenSource cts = new();
		_ = s.StartFor(cts.Token);

		await secondIteration.Task.WaitAsync(TEST_TIMEOUT);
		cts.Cancel();

		s.Exceptions.Should().BeGreaterThan(0);
		s.Iterations.Should().BeGreaterThanOrEqualTo(2);
	}
}
