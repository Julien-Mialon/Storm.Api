namespace Storm.Api.Extensions;

public static class SemaphoreExtensions
{
	public static async Task<IDisposable> Lock(this SemaphoreSlim semaphore)
	{
		await semaphore.WaitAsync();
		return new LockDisposable(semaphore);
	}

	private class LockDisposable : IDisposable
	{
		private readonly SemaphoreSlim _semaphore;
		private bool _disposed;

		public LockDisposable(SemaphoreSlim semaphore)
		{
			_semaphore = semaphore;
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;
			_semaphore.Release();
		}
	}
}