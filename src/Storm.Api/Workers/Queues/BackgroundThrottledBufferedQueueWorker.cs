using Storm.Api.Logs;
using Storm.Api.Queues;
using Storm.Api.Workers.Strategies;

namespace Storm.Api.Workers.Queues;

public class BackgroundThrottledBufferedQueueWorker<TWorkItem> : AbstractBackgroundQueueWorker<TWorkItem, TWorkItem[]> where TWorkItem : class
{
	public BackgroundThrottledBufferedQueueWorker(ILogService logService, Func<IReadOnlyList<TWorkItem>, Task<bool>> itemAction, TimeSpan throttlingTime, Action<IReadOnlyList<TWorkItem>, Exception>? onException, int bufferSize, IRetryStrategy? retryStrategy)
		: base(logService, new ThrottledBufferedItemQueue<TWorkItem>(bufferSize, throttlingTime), itemAction, onException, retryStrategy)
	{
	}
}