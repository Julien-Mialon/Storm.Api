using Storm.Api.Logs;
using Storm.Api.Logs.Extensions;
using Storm.Api.Queues;
using Storm.Api.Workers.Strategies;

namespace Storm.Api.Workers.Queues;

public abstract class AbstractBackgroundQueueWorker<TInput, TOutput> : IWorker<TInput> where TInput : class
{
	private readonly ILogService _logService;
	private readonly BackgroundWorker _worker;

	private readonly IItemQueue<TInput, TOutput> _queue;

	private readonly Func<TOutput, Task<bool>> _itemAction;
	private readonly Action<TOutput, Exception>? _onException;

	private readonly IRetryStrategy? _retryStrategy;

	protected AbstractBackgroundQueueWorker(ILogService logService, IItemQueue<TInput, TOutput> queue, Func<TOutput, Task<bool>> itemAction, Action<TOutput, Exception>? onException, IRetryStrategy? retryStrategy)
	{
		_logService = logService;
		_itemAction = itemAction;
		_onException = onException;
		_retryStrategy = retryStrategy;
		_queue = queue;

		_worker = new(_logService, RunAsync);
		_worker.Start();
	}

	public void Queue(TInput item)
	{
		_queue.Queue(item);
		_worker.Start();
	}

	private async Task RunAsync(CancellationToken ct)
	{
		while (ct.IsCancellationRequested is false)
		{
			TOutput items = await _queue.Dequeue(ct);
			await ProcessItemsAsync(items, ct);
		}
	}

	private async Task ProcessItemsAsync(TOutput items, CancellationToken ct)
	{
		for (int tries = 0 ; ; tries++)
		{
			try
			{
				if (await _itemAction.Invoke(items))
				{
					OnItemsSuccess(items);
					return;
				}
			}
			catch (Exception ex)
			{
				_logService.Error(x => x
					.WriteProperty("type", GetType().FullName)
					.WriteMethodInfo()
					.WriteException(ex));

				_onException?.Invoke(items, ex);
			}

			if (_retryStrategy?.DiscardAfterFailedAttempts is true && tries >= _retryStrategy.AttemptsCountBeforeDiscard)
			{
				int attemptsCount = tries + 1;
				_logService.Error(x => x
					.WriteProperty("type", GetType().FullName)
					.WriteMethodInfo()
					.WriteMessage($"Discarding item after {attemptsCount} failed attempts")
					.DumpObject("items", items));
				return;
			}

			await OnItemsError(items);
		}
	}

	protected virtual void OnItemsSuccess(TOutput item)
	{
		_retryStrategy?.Reset();
	}

	protected virtual Task OnItemsError(TOutput item)
	{
		return _retryStrategy?.Wait() ?? Task.CompletedTask;
	}
}