namespace Storm.Api.Queues;

public interface IItemQueue<in TInput, TOutput>
{
	void Queue(TInput item);
	Task<TOutput> Dequeue(CancellationToken ct);
}

public interface IItemQueue<TWorkItem> : IItemQueue<TWorkItem, TWorkItem>;