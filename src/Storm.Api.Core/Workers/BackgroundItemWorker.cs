using System.Collections.Concurrent;
using Storm.Api.Core.Extensions;
using Storm.Api.Core.Logs;

namespace Storm.Api.Core.Workers;

public abstract class BackgroundItemWorker<TWorkItem> : IWorker<TWorkItem>
	where TWorkItem : class
{
	private readonly ILogService _logService;
	private readonly BackgroundWorker _worker;

	private readonly ConcurrentQueue<TWorkItem> _waitingQueue = new ConcurrentQueue<TWorkItem>();
	private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);

	private readonly Func<TWorkItem, Task<bool>> _itemAction;
	private readonly Action<TWorkItem?, Exception>? _onException;
	private readonly int? _discardAfterFailAttemptsCount;

	protected BackgroundItemWorker(ILogService logService, Func<TWorkItem, Task<bool>> itemAction, Action<TWorkItem?, Exception>? onException, int? discardAfterFailAttemptsCount)
	{
		_logService = logService;
		_itemAction = itemAction;
		_onException = onException;
		_discardAfterFailAttemptsCount = discardAfterFailAttemptsCount;

		_worker = new BackgroundWorker(_logService, RunAsync);
		_worker.Start();
	}

	public void Queue(TWorkItem item)
	{
		_waitingQueue.Enqueue(item);
		_semaphore.Release();
		_worker.Start();
	}

	private async Task RunAsync(CancellationToken ct)
	{
		TWorkItem? item = null;
		int tries = 0;

		while (true)
		{
			await _semaphore.WaitAsync(ct);
			ct.ThrowIfCancellationRequested();

			try
			{
				if (_waitingQueue.TryPeek(out item))
				{
					if (await _itemAction.Invoke(item))
					{
						_waitingQueue.TryDequeue(out _);
						OnItemSuccess(item);
						tries = 0;
					}
					else
					{
						if (_discardAfterFailAttemptsCount is null)
						{
							await OnItemError(item);
							_semaphore.Release();
						}
						else
						{
							tries++;
							if (tries >= _discardAfterFailAttemptsCount)
							{
								_logService.Error(x => x
									.WriteProperty("type", GetType().FullName)
									.WriteMethodInfo()
									.WriteMessage($"Fail to process item {tries} times")
									.DumpObject("item", item)
								);

								_waitingQueue.TryDequeue(out _);
								tries = 0;
							}
							else
							{
								await OnItemError(item);
								_semaphore.Release();
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logService.Error(x => x
					.WriteProperty("type", GetType().FullName)
					.WriteMethodInfo()
					.WriteException(ex)
				);

				_onException?.Invoke(item, ex);
			}
		}
	}

	protected virtual void OnItemSuccess(TWorkItem item)
	{

	}

	protected virtual Task OnItemError(TWorkItem item)
	{
		return Task.CompletedTask;
	}
}