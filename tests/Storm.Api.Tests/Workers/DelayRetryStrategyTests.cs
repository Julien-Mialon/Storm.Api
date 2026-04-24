using System.Diagnostics;
using Storm.Api.Workers.Strategies;

namespace Storm.Api.Tests.Workers;

public class DelayRetryStrategyTests
{
	[Fact]
	public async Task Wait_DelaysForConfiguredMilliseconds()
	{
		DelayRetryStrategy s = new(50, null);
		Stopwatch sw = Stopwatch.StartNew();
		await s.Wait();
		sw.Stop();
		sw.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(40);
	}

	[Fact]
	public void DiscardAfterFailedAttempts_SetWhenCountProvided()
	{
		DelayRetryStrategy s = new(10, 5);
		s.DiscardAfterFailedAttempts.Should().BeTrue();
		s.AttemptsCountBeforeDiscard.Should().Be(5);

		DelayRetryStrategy none = new(10, null);
		none.DiscardAfterFailedAttempts.Should().BeFalse();
		none.AttemptsCountBeforeDiscard.Should().Be(0);
	}

	[Fact]
	public void Reset_IsNoOp_ConstantDelay()
	{
		DelayRetryStrategy s = new(10, null);
		Action act = () => s.Reset();
		act.Should().NotThrow();
	}
}
