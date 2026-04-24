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

	private sealed class CountingService(IServiceProvider services, TimeSpan interval, Func<IServiceProvider, Task>? body = null)
		: BasePeriodicRunHostedService(services, interval)
	{
		public int Iterations;
		public int Exceptions;
		private readonly Func<IServiceProvider, Task> _body = body ?? (_ => Task.CompletedTask);

		protected override async Task Run(IServiceProvider services)
		{
			Interlocked.Increment(ref Iterations);
			await _body(services);
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

	[Fact]
	public async Task ExecuteAsync_CallsRunRepeatedly_RespectingInterval()
	{
		CountingService s = new(Provider(), TimeSpan.FromMilliseconds(30));
		using CancellationTokenSource cts = new();
		_ = s.StartFor(cts.Token);
		await Task.Delay(200);
		cts.Cancel();
		s.Iterations.Should().BeGreaterThan(1);
	}

	[Fact]
	public async Task ExecuteAsync_CreatesAndDisposesScopePerIteration()
	{
		int scopesSeen = 0;
		CountingService s = new(Provider(), TimeSpan.FromMilliseconds(30), sp =>
		{
			sp.Should().NotBeNull();
			Interlocked.Increment(ref scopesSeen);
			return Task.CompletedTask;
		});
		using CancellationTokenSource cts = new();
		_ = s.StartFor(cts.Token);
		await Task.Delay(200);
		cts.Cancel();
		scopesSeen.Should().BeGreaterThan(0);
	}

	[Fact]
	public async Task ExecuteAsync_CancellationStopsLoop()
	{
		CountingService s = new(Provider(), TimeSpan.FromMilliseconds(20));
		using CancellationTokenSource cts = new();
		_ = s.StartFor(cts.Token);
		await Task.Delay(100);
		cts.Cancel();
		await Task.Delay(50);
		int afterCancel = s.Iterations;
		await Task.Delay(100);
		s.Iterations.Should().Be(afterCancel);
	}

	[Fact]
	public async Task ExecuteAsync_ExceptionInRun_LoggedAndLoopContinues()
	{
		int calls = 0;
		CountingService s = new(Provider(), TimeSpan.FromMilliseconds(20), _ =>
		{
			if (++calls == 1)
			{
				throw new InvalidOperationException("boom");
			}
			return Task.CompletedTask;
		});
		using CancellationTokenSource cts = new();
		_ = s.StartFor(cts.Token);
		await Task.Delay(200);
		cts.Cancel();
		s.Exceptions.Should().BeGreaterThan(0);
	}
}
