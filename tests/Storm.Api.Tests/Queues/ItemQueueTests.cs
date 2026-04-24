using Storm.Api.Queues;

namespace Storm.Api.Tests.Queues;

public class ItemQueueTests
{
	[Fact]
	public async Task Queue_SingleItem_CanBeDequeued()
	{
		ItemQueue<int> q = new();
		q.Queue(5);
		(await q.Dequeue(CancellationToken.None)).Should().Be(5);
	}

	[Fact]
	public async Task Queue_MultipleItems_DequeuedInFifoOrder()
	{
		ItemQueue<int> q = new();
		q.Queue(1);
		q.Queue(2);
		q.Queue(3);
		(await q.Dequeue(CancellationToken.None)).Should().Be(1);
		(await q.Dequeue(CancellationToken.None)).Should().Be(2);
		(await q.Dequeue(CancellationToken.None)).Should().Be(3);
	}

	[Fact]
	public async Task Dequeue_EmptyQueue_BlocksUntilItemAvailable()
	{
		ItemQueue<int> q = new();
		Task<int> t = q.Dequeue(CancellationToken.None);
		t.IsCompleted.Should().BeFalse();
		q.Queue(42);
		(await t).Should().Be(42);
	}

	[Fact]
	public async Task Dequeue_CancellationTokenCancelled_ThrowsOperationCanceled()
	{
		ItemQueue<int> q = new();
		using CancellationTokenSource cts = new();
		Task<int> t = q.Dequeue(cts.Token);
		cts.Cancel();
		Func<Task> act = () => t;
		await act.Should().ThrowAsync<OperationCanceledException>();
	}

	[Fact]
	public async Task DequeueAll_IteratesAllQueuedItems()
	{
		ItemQueue<int> q = new();
		q.Queue(1);
		q.Queue(2);

		using CancellationTokenSource cts = new();
		List<int> collected = [];
		cts.CancelAfter(200);
		try
		{
			await foreach (int v in q.DequeueAll(cts.Token))
			{
				collected.Add(v);
				if (collected.Count == 2)
				{
					break;
				}
			}
		}
		catch (OperationCanceledException)
		{
		}
		collected.Should().Equal(1, 2);
	}

	[Fact]
	public async Task ConcurrentQueueAndDequeue_IsThreadSafe()
	{
		ItemQueue<int> q = new();
		Task producer = Task.Run(() =>
		{
			for (int i = 0; i < 100; i++)
			{
				q.Queue(i);
			}
		});

		List<int> got = [];
		for (int i = 0; i < 100; i++)
		{
			got.Add(await q.Dequeue(CancellationToken.None));
		}
		await producer;
		got.Should().HaveCount(100);
		got.Should().BeEquivalentTo(Enumerable.Range(0, 100));
	}
}
