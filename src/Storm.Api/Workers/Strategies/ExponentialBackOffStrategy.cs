namespace Storm.Api.Workers.Strategies;

public class ExponentialBackOffStrategy : IRetryStrategy
{
	private readonly int _baseMillisecondsCount;
	private readonly int _maxIteration;
	private int _currentIteration;

	public bool DiscardAfterFailedAttempts { get; }
	public int AttemptsCountBeforeDiscard { get; }

	public ExponentialBackOffStrategy(int baseMillisecondsCount, int maxIteration, int? attemptsCountBeforeDiscard)
	{
		_baseMillisecondsCount = baseMillisecondsCount;
		_maxIteration = maxIteration;
		DiscardAfterFailedAttempts = attemptsCountBeforeDiscard.HasValue;
		AttemptsCountBeforeDiscard = attemptsCountBeforeDiscard ?? 0;
	}

	public void Reset()
	{
		_currentIteration = 0;
	}

	public Task Wait()
	{
		_currentIteration++;
		if (_currentIteration > _maxIteration)
		{
			_currentIteration = _maxIteration;
		}

		return Task.Delay(GetTimeForCurrentIteration());
	}

	private int GetTimeForCurrentIteration()
	{
		return _currentIteration * (_currentIteration + 1) / 2 * _baseMillisecondsCount;
	}
}