using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Storm.Api.Core.Logs;

namespace Storm.Api.Services
{
	public abstract class BackgroundServiceWorker<TItem> : BackgroundService
	{
		private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0, 1_000_000);
		private readonly ConcurrentQueue<TItem> _queue = new ConcurrentQueue<TItem>();
		protected ILogService LogService { get; }

		public BackgroundServiceWorker(ILogService logService)
		{
			LogService = logService;
		}

		public void Add(TItem item)
		{
			_queue.Enqueue(item);
			_semaphore.Release();
		}


		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (true)
			{
				if (stoppingToken.IsCancellationRequested)
				{
					break;
				}

				await _semaphore.WaitAsync(stoppingToken);

				if (stoppingToken.IsCancellationRequested)
				{
					break;
				}

				try
				{
					if (_queue.TryDequeue(out TItem item))
					{
						try
						{
							await ProcessItem(item);
						}
						catch (Exception)
						{
							_queue.Enqueue(item); //in case of error, re-add it to the queue
							throw;
						}
					}
				}
				catch (Exception)
				{
					_semaphore.Release();
				}
			}
		}

		protected abstract Task ProcessItem(TItem item);
	}
}