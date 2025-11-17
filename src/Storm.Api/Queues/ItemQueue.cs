using System.Threading.Channels;

namespace Storm.Api.Queues;

public class ItemQueue<TWorkItem> : IItemQueue<TWorkItem>
{
	private readonly Channel<TWorkItem> _queue = Channel.CreateUnbounded<TWorkItem>();

	public void Queue(TWorkItem item)
	{
		_queue.Writer.TryWrite(item);
	}

	public async Task<TWorkItem> Dequeue(CancellationToken ct)
	{
		return await _queue.Reader.ReadAsync(ct);
	}

	public IAsyncEnumerable<TWorkItem> DequeueAll(CancellationToken ct)
	{
		return _queue.Reader.ReadAllAsync(ct);
	}
}