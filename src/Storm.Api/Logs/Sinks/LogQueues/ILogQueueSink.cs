namespace Storm.Api.Logs.Sinks.LogQueues;

public interface ILogQueueSink
{
	Task<string?> Next(TimeSpan timeout);
}