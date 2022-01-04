namespace Storm.Api.Core.Workers;

internal class ExponentialBackOffStrategy
{
	private readonly int _baseMillisecondsCount;
	private readonly int _maxIteration;
	private int _currentIteration;

	public ExponentialBackOffStrategy(int baseMillisecondsCount, int maxIteration)
	{
		_baseMillisecondsCount = baseMillisecondsCount;
		_maxIteration = maxIteration;
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