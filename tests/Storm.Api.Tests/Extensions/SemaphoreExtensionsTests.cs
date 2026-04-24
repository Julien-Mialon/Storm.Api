using Storm.Api.Extensions;

namespace Storm.Api.Tests.Extensions;

public class SemaphoreExtensionsTests
{
	[Fact]
	public async Task Lock_AcquiresSemaphore()
	{
		SemaphoreSlim s = new(1, 1);
		using IDisposable d = await s.Lock();
		s.CurrentCount.Should().Be(0);
	}

	[Fact]
	public async Task Lock_DisposeReleasesSemaphoreOnce()
	{
		SemaphoreSlim s = new(1, 1);
		IDisposable d = await s.Lock();
		d.Dispose();
		s.CurrentCount.Should().Be(1);
	}

	[Fact]
	public async Task Lock_DoubleDispose_ReleasesOnlyOnce()
	{
		SemaphoreSlim s = new(1, 1);
		IDisposable d = await s.Lock();
		d.Dispose();
		d.Dispose();
		s.CurrentCount.Should().Be(1);
	}

	[Fact]
	public async Task Lock_SerializesConcurrentAccess()
	{
		SemaphoreSlim s = new(1, 1);
		int counter = 0;
		int max = 0;

		async Task Worker()
		{
			using (await s.Lock())
			{
				int current = Interlocked.Increment(ref counter);
				if (current > max)
				{
					max = current;
				}
				await Task.Delay(10);
				Interlocked.Decrement(ref counter);
			}
		}

		await Task.WhenAll(Enumerable.Range(0, 5).Select(_ => Worker()));
		max.Should().Be(1);
	}
}
