using Storm.Api.Queues;

namespace Storm.Api.Tests.Queues;

public class BufferedItemQueueTests
{
	[Fact]
	public async Task Dequeue_ReturnsUpToBufferSizeItems()
	{
		BufferedItemQueue<int> q = new(bufferSize: 3);
		q.Queue(1);
		q.Queue(2);
		q.Queue(3);
		q.Queue(4);

		int[] items = await q.Dequeue(CancellationToken.None);
		items.Should().Equal(1, 2, 3);
	}

	[Fact]
	public async Task Dequeue_ExactlyBufferSizeWhenEnoughAvailable()
	{
		BufferedItemQueue<int> q = new(bufferSize: 2);
		q.Queue(10);
		q.Queue(20);
		int[] items = await q.Dequeue(CancellationToken.None);
		items.Should().Equal(10, 20);
	}

	[Fact]
	public async Task Dequeue_BlocksUntilAtLeastOneItem()
	{
		BufferedItemQueue<int> q = new(bufferSize: 1);
		Task<int[]> t = q.Dequeue(CancellationToken.None);
		t.IsCompleted.Should().BeFalse();
		q.Queue(42);
		int[] items = await t;
		items.Should().Equal(42);
	}

	[Fact]
	public async Task Dequeue_CancellationReturnsPartialBuffer()
	{
		BufferedItemQueue<int> q = new(bufferSize: 5);
		q.Queue(1);
		using CancellationTokenSource cts = new();
		Task<int[]> t = q.Dequeue(cts.Token);
		await Task.Delay(30);
		cts.Cancel();
		Func<Task> act = () => t;
		await act.Should().ThrowAsync<OperationCanceledException>();
	}

	[Fact]
	public async Task Dequeue_ConcurrentQueueAndDequeue_IsThreadSafe()
	{
		BufferedItemQueue<int> q = new(bufferSize: 50);
		Task producer = Task.Run(() =>
		{
			for (int i = 0; i < 50; i++)
			{
				q.Queue(i);
			}
		});
		int[] got = await q.Dequeue(CancellationToken.None);
		await producer;
		got.Should().HaveCount(50);
	}
}
