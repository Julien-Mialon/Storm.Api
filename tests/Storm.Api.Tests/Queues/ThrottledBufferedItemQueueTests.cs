using Storm.Api.Queues;

namespace Storm.Api.Tests.Queues;

public class ThrottledBufferedItemQueueTests
{
	[Fact]
	public async Task Dequeue_FirstItem_WaitsIndefinitely()
	{
		ThrottledBufferedItemQueue<int> q = new(10, TimeSpan.FromMilliseconds(50));
		Task<int[]> t = q.Dequeue(CancellationToken.None);
		await Task.Delay(100);
		t.IsCompleted.Should().BeFalse();
		q.Queue(1);
		int[] items = await t;
		items.Should().Equal(1);
	}

	[Fact]
	public async Task Dequeue_SubsequentItems_ConstrainedByThrottlingTime()
	{
		ThrottledBufferedItemQueue<int> q = new(10, TimeSpan.FromMilliseconds(50));
		q.Queue(1);
		q.Queue(2);
		int[] items = await q.Dequeue(CancellationToken.None);
		items.Should().BeEquivalentTo(new[] { 1, 2 });
	}

	[Fact]
	public async Task Dequeue_TimeoutAfterFirstItem_ReturnsPartialBuffer()
	{
		ThrottledBufferedItemQueue<int> q = new(10, TimeSpan.FromMilliseconds(50));
		q.Queue(1);
		int[] items = await q.Dequeue(CancellationToken.None);
		items.Should().Equal(1);
	}

	[Fact]
	public async Task Dequeue_AllArriveBeforeTimeout_ReturnsFullBuffer()
	{
		ThrottledBufferedItemQueue<int> q = new(3, TimeSpan.FromSeconds(5));
		q.Queue(1);
		q.Queue(2);
		q.Queue(3);
		int[] items = await q.Dequeue(CancellationToken.None);
		items.Should().Equal(1, 2, 3);
	}

	[Fact]
	public async Task Dequeue_CancelledBeforeFirstItem_ReturnsEmptyArray()
	{
		ThrottledBufferedItemQueue<int> q = new(3, TimeSpan.FromMilliseconds(50));
		using CancellationTokenSource cts = new();
		Task<int[]> t = q.Dequeue(cts.Token);
		cts.Cancel();
		int[] items = await t;
		items.Should().BeEmpty();
	}

	[Fact]
	public async Task Dequeue_ThrottlingTimeoutTriggersOperationCancelled_SwallowedAndReturnsPartial()
	{
		// Queuing one item and setting a very short throttle forces the inner `ReadAsync` call to throw
		// OperationCanceledException when the throttle elapses — that exception is swallowed by the
		// broad catch in Dequeue() and a partial buffer is returned. Pins current behavior: see source
		// comment in ThrottledBufferedItemQueue.Dequeue.
		ThrottledBufferedItemQueue<int> q = new(10, TimeSpan.FromMilliseconds(20));
		q.Queue(1);
		int[] items = await q.Dequeue(CancellationToken.None);
		items.Should().Equal(1);
	}

	[Fact]
	public async Task Dequeue_ExternalCancellationAfterFirstItem_ReturnsEmptyArray()
	{
		// Outer ct is cancelled after one item arrives; the inner ReadAsync throws.
		// Current behavior returns [] because ct.IsCancellationRequested is true.
		ThrottledBufferedItemQueue<int> q = new(10, TimeSpan.FromSeconds(5));
		using CancellationTokenSource cts = new();
		q.Queue(1);
		Task<int[]> t = q.Dequeue(cts.Token);
		await Task.Delay(30);
		cts.Cancel();
		int[] items = await t;
		items.Should().BeEmpty();
	}
}
