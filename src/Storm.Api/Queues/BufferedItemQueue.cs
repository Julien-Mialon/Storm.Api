using System.Threading.Channels;

namespace Storm.Api.Queues;

public class BufferedItemQueue<TWorkItem> : IBufferedItemQueue<TWorkItem>
{
	private readonly int _bufferSize;
	private readonly Channel<TWorkItem> _queue = Channel.CreateUnbounded<TWorkItem>();

	public BufferedItemQueue(int bufferSize)
	{
		_bufferSize = bufferSize;
	}

	public void Queue(TWorkItem item)
	{
		_queue.Writer.TryWrite(item);
	}

	public async Task<TWorkItem[]> Dequeue(CancellationToken ct)
	{
		TWorkItem[] items = new TWorkItem[_bufferSize];
		int bufferIndex = 0;

		await foreach (TWorkItem workItem in _queue.Reader.ReadAllAsync(ct))
		{
			items[bufferIndex++] = workItem;
			if (bufferIndex >= _bufferSize)
			{
				break;
			}
		}

		return items;
	}
}