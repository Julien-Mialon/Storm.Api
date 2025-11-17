using Storm.Api.Logs;
using Storm.Api.Queues;
using Storm.Api.Workers.Strategies;

namespace Storm.Api.Workers.Queues;

public class BackgroundQueueWorker<TWorkItem> : AbstractBackgroundQueueWorker<TWorkItem, TWorkItem> where TWorkItem : class
{
	public BackgroundQueueWorker(ILogService logService, Func<TWorkItem, Task<bool>> itemAction, Action<TWorkItem?, Exception>? onException, IRetryStrategy? retryStrategy)
		: base(logService, new ItemQueue<TWorkItem>(), itemAction, onException, retryStrategy)
	{
	}

	public BackgroundQueueWorker(ILogService logService, IItemQueue<TWorkItem> queue, Func<TWorkItem, Task<bool>> itemAction, Action<TWorkItem?, Exception>? onException, IRetryStrategy? retryStrategy)
		: base(logService, queue, itemAction, onException, retryStrategy)
	{
	}
}