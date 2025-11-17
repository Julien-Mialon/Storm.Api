using System.Threading.Channels;

namespace Storm.Api.Queues;

public class ThrottledBufferedItemQueue<TWorkItem> : IBufferedItemQueue<TWorkItem>
{
	private readonly int _bufferSize;
	private readonly TimeSpan _throttlingTime;
	private readonly Channel<TWorkItem> _queue = Channel.CreateUnbounded<TWorkItem>();

	public ThrottledBufferedItemQueue(int bufferSize, TimeSpan throttlingTime)
	{
		_bufferSize = bufferSize;
		_throttlingTime = throttlingTime;
	}

	public void Queue(TWorkItem item)
	{
		_queue.Writer.TryWrite(item);
	}

	public async Task<TWorkItem[]> Dequeue(CancellationToken ct)
	{
		TWorkItem[] items = new TWorkItem[_bufferSize];
		int bufferIndex = 0;

		try
		{
			while (bufferIndex < _bufferSize)
			{
				if (bufferIndex == 0)
				{
					items[bufferIndex] = await _queue.Reader.ReadAsync(ct);
					bufferIndex++;
				}
				else
				{
					using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
					cts.CancelAfter(_throttlingTime);
					items[bufferIndex] = await _queue.Reader.ReadAsync(cts.Token);
					bufferIndex++;
				}
			}
		}
		catch (Exception)
		{
			// ignored
		}

		if (bufferIndex > 0 && ct.IsCancellationRequested is false)
		{
			if (bufferIndex == _bufferSize)
			{
				return items;
			}

			TWorkItem[] itemsCopy = items[..bufferIndex];
			return itemsCopy;
		}

		return [];
	}
}