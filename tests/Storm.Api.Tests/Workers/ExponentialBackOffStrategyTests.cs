using System.Diagnostics;
using System.Reflection;
using Storm.Api.Workers.Strategies;

namespace Storm.Api.Tests.Workers;

public class ExponentialBackOffStrategyTests
{
	private static int Iter(ExponentialBackOffStrategy s)
	{
		FieldInfo f = typeof(ExponentialBackOffStrategy).GetField("_currentIteration", BindingFlags.NonPublic | BindingFlags.Instance)!;
		return (int)f.GetValue(s)!;
	}

	[Fact]
	public async Task Wait_Iteration1_DelayEqualsBase()
	{
		ExponentialBackOffStrategy s = new(50, maxIteration: 10, attemptsCountBeforeDiscard: null);
		Stopwatch sw = Stopwatch.StartNew();
		await s.Wait();
		sw.Stop();
		Iter(s).Should().Be(1);
		sw.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(40);
	}

	[Fact]
	public async Task Wait_Iteration2_DelayEqualsThreeTimesBase()
	{
		ExponentialBackOffStrategy s = new(30, maxIteration: 10, attemptsCountBeforeDiscard: null);
		await s.Wait();
		Stopwatch sw = Stopwatch.StartNew();
		await s.Wait();
		sw.Stop();
		Iter(s).Should().Be(2);
		sw.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(80);
	}

	[Fact]
	public async Task Wait_IterationCapsAtMaxIteration()
	{
		ExponentialBackOffStrategy s = new(1, maxIteration: 2, attemptsCountBeforeDiscard: null);
		await s.Wait();
		await s.Wait();
		await s.Wait();
		await s.Wait();
		Iter(s).Should().Be(2);
	}

	[Fact]
	public async Task Reset_ResetsIterationCounter()
	{
		ExponentialBackOffStrategy s = new(1, 10, null);
		await s.Wait();
		await s.Wait();
		s.Reset();
		Iter(s).Should().Be(0);
	}

	[Fact]
	public void DiscardAfterFailedAttempts_SetWhenCountProvided()
	{
		new ExponentialBackOffStrategy(1, 10, 5).DiscardAfterFailedAttempts.Should().BeTrue();
		new ExponentialBackOffStrategy(1, 10, null).DiscardAfterFailedAttempts.Should().BeFalse();
	}
}
