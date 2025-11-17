using Storm.Api.Logs;
using Storm.Api.Queues;
using Storm.Api.Workers.Strategies;

namespace Storm.Api.Workers.Queues;

public class BackgroundBufferedQueueWorker<TWorkItem> : AbstractBackgroundQueueWorker<TWorkItem, TWorkItem[]> where TWorkItem : class
{
	public BackgroundBufferedQueueWorker(ILogService logService, Func<IReadOnlyList<TWorkItem>, Task<bool>> itemAction, Action<IReadOnlyList<TWorkItem>, Exception>? onException, int bufferSize, IRetryStrategy? retryStrategy)
		: base(logService, new BufferedItemQueue<TWorkItem>(bufferSize), itemAction, onException, retryStrategy)
	{
	}

	public BackgroundBufferedQueueWorker(ILogService logService, IItemQueue<TWorkItem, TWorkItem[]> queue, Func<IReadOnlyList<TWorkItem>, Task<bool>> itemAction, Action<IReadOnlyList<TWorkItem>, Exception>? onException, IRetryStrategy? retryStrategy)
		: base(logService, queue, itemAction, onException, retryStrategy)
	{
	}
}