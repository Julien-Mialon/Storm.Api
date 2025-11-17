namespace Storm.Api.Workers.Strategies;

public class DelayRetryStrategy : IRetryStrategy
{
	private readonly int _timeToWait;

	public bool DiscardAfterFailedAttempts { get; }
	public int AttemptsCountBeforeDiscard { get; }

	public DelayRetryStrategy(int timeToWait, int? attemptsCountBeforeDiscard)
	{
		_timeToWait = timeToWait;
		DiscardAfterFailedAttempts = attemptsCountBeforeDiscard.HasValue;
		AttemptsCountBeforeDiscard = attemptsCountBeforeDiscard ?? 0;
	}

	public void Reset()
	{
	}

	public Task Wait()
	{
		return Task.Delay(_timeToWait);
	}
}