using Storm.Api.Core.Workers;

namespace Storm.Api.Core.Logs.Senders;

public abstract class AbstractImmediateQueueLogSender : ILogSender
{
	private readonly BackgroundItemQueueWorker<string> _worker;

	protected AbstractImmediateQueueLogSender(ILogService logService)
	{
		_worker = new BackgroundItemQueueWorker<string>(logService, Send);
	}

	public void Enqueue(LogLevel level, string entry)
	{
		_worker.Queue(entry);
	}

	protected abstract Task<bool> Send(string entry);
}