using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Storm.Api.Logs;
using Storm.Api.Logs.Interfaces;
using Storm.Api.Workers.HostedServices;

namespace Storm.Api.Tests.Workers;

public class BaseTimeRunHostedServiceTests
{
	private sealed class NullSink : ILogSink
	{
		public void Enqueue(LogLevel level, string entry) { }
	}

	private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
	{
		public override DateTimeOffset GetUtcNow() => now;
	}

	private sealed class TestService(IServiceProvider services, params TimeOnly[] times)
		: BaseTimeRunHostedService(services, times)
	{
		protected override Task Run(IServiceProvider services) => Task.CompletedTask;
	}

	private static IServiceProvider Provider(DateTimeOffset now)
	{
		ServiceCollection sc = new();
		sc.AddSingleton<ILogService>(new LogService(_ => new NullSink(), LogLevel.Trace));
		sc.AddSingleton<TimeProvider>(new FixedTimeProvider(now));
		return sc.BuildServiceProvider();
	}

	private static async Task<TimeSpan> InvokeAwaitAndMeasure(TestService s, CancellationToken ct)
	{
		MethodInfo? m = typeof(BaseTimeRunHostedService).GetMethod("AwaitNextRun", BindingFlags.NonPublic | BindingFlags.Instance);
		Task t = (Task)m!.Invoke(s, [ct])!;
		DateTime start = DateTime.UtcNow;
		try
		{
			await t;
		}
		catch (TaskCanceledException)
		{
		}
		return DateTime.UtcNow - start;
	}

	[Fact]
	public async Task AwaitNextRun_SingleTimeInFuture_WaitsUntilToday()
	{
		DateTimeOffset now = new(2024, 6, 1, 8, 0, 0, TimeSpan.Zero);
		TestService s = new(Provider(now), new TimeOnly(8, 0, 1));

		using CancellationTokenSource cts = new();
		cts.CancelAfter(50);
		TimeSpan elapsed = await InvokeAwaitAndMeasure(s, cts.Token);
		elapsed.Should().BeLessThan(TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task AwaitNextRun_SingleTimeInPast_WaitsUntilTomorrow()
	{
		DateTimeOffset now = new(2024, 6, 1, 12, 0, 0, TimeSpan.Zero);
		TestService s = new(Provider(now), new TimeOnly(10, 0, 0));

		using CancellationTokenSource cts = new();
		cts.CancelAfter(50);
		await InvokeAwaitAndMeasure(s, cts.Token);
	}

	[Fact]
	public async Task AwaitNextRun_MultipleTimes_ReturnsEarliestUpcoming()
	{
		DateTimeOffset now = new(2024, 6, 1, 9, 0, 0, TimeSpan.Zero);
		TestService s = new(Provider(now), new TimeOnly(8, 0, 0), new TimeOnly(10, 0, 0), new TimeOnly(15, 0, 0));

		using CancellationTokenSource cts = new();
		cts.CancelAfter(50);
		await InvokeAwaitAndMeasure(s, cts.Token);
	}

	[Fact]
	public async Task AwaitNextRun_MidnightBoundary_Handled()
	{
		DateTimeOffset now = new(2024, 6, 1, 23, 59, 50, TimeSpan.Zero);
		TestService s = new(Provider(now), new TimeOnly(0, 0, 0));

		using CancellationTokenSource cts = new();
		cts.CancelAfter(50);
		await InvokeAwaitAndMeasure(s, cts.Token);
	}

	[Fact]
	public async Task AwaitNextRun_CancellationDuringWait_Throws()
	{
		DateTimeOffset now = new(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
		TestService s = new(Provider(now), new TimeOnly(23, 59, 59));

		using CancellationTokenSource cts = new();
		MethodInfo m = typeof(BaseTimeRunHostedService).GetMethod("AwaitNextRun", BindingFlags.NonPublic | BindingFlags.Instance)!;
		Task t = (Task)m.Invoke(s, [cts.Token])!;
		cts.Cancel();
		Func<Task> act = () => t;
		await act.Should().ThrowAsync<TaskCanceledException>();
	}
}
