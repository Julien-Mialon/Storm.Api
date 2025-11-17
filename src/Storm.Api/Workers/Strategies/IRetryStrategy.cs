namespace Storm.Api.Workers.Strategies;

public interface IRetryStrategy
{
	bool DiscardAfterFailedAttempts { get; }
	int AttemptsCountBeforeDiscard { get; }
	void Reset();
	Task Wait();
}