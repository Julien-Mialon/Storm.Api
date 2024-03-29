using System.Collections.Concurrent;

namespace Storm.Api.Core.Logs;

public interface ILogQueueService
{
	Task<string?> Next(TimeSpan timeout);
}

public class LogQueueService : LogService, ILogQueueService, ILogSender
{
	private readonly ConcurrentQueue<string> _logs = new();
	private readonly SemaphoreSlim _semaphore = new(0);

	public LogQueueService(LogLevel minimumLogLevel) : base(minimumLogLevel)
	{
		UseSender(this);
	}

	public void Enqueue(LogLevel level, string entry)
	{
		_logs.Enqueue(entry);
		_semaphore.Release();
	}

	public async Task<string?> Next(TimeSpan timeout)
	{
		if (!await _semaphore.WaitAsync(timeout))
		{
			return null;
		}
		if (_logs.TryDequeue(out string result))
		{
			return result;
		}

		_semaphore.Release();
		return null;
	}
}