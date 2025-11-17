using Storm.Api.Logs;
using Storm.Api.Logs.Extensions;
using Storm.Api.Queues;
using Storm.Api.Workers.Strategies;

namespace Storm.Api.Workers.HostedServices;

public abstract class AbstractHostedServiceQueueWorker<TInput, TOutput, TQueue> : BaseHostedService
	where TInput : class
	where TOutput : class
	where TQueue : class, IItemQueue<TInput, TOutput>
{
	private readonly IRetryStrategy? _retryStrategy;

	protected AbstractHostedServiceQueueWorker(IServiceProvider services, IRetryStrategy? retryStrategy = null)
		: base(services)
	{
		_retryStrategy = retryStrategy;
	}

	protected override async Task ExecuteAsync(CancellationToken ct)
	{
		TQueue queue = Resolve<TQueue>();

		while (ct.IsCancellationRequested is false)
		{
			TOutput items = await queue.Dequeue(ct);
			await ProcessItemsAsync(items, ct);
		}
	}

	private async Task ProcessItemsAsync(TOutput items, CancellationToken ct)
	{
		for (int tries = 0 ; ; tries++)
		{
			try
			{
				if (await DoAction(items))
				{
					OnItemsSuccess(items);
					return;
				}

				if (_retryStrategy?.DiscardAfterFailedAttempts is not true || tries < _retryStrategy.AttemptsCountBeforeDiscard)
				{
					await OnItemsError(items);
				}
				else
				{
					int triesCopy = tries;
					Resolve<ILogService>().Error(x => x
						.WriteProperty("type", GetType().FullName)
						.WriteMethodInfo()
						.WriteMessage($"Fail to process item {triesCopy} times")
						.DumpObject("items", items)
					);
				}
			}
			catch (Exception ex)
			{
				Resolve<ILogService>().Error(x => x
					.WriteProperty("type", GetType().FullName)
					.WriteMethodInfo()
					.WriteException(ex)
				);

				await OnItemsError(items);

				if (_retryStrategy?.DiscardAfterFailedAttempts is true && tries >= _retryStrategy.AttemptsCountBeforeDiscard)
				{
					return;
				}
			}
		}
	}

	protected abstract Task<bool> DoAction(TOutput item);

	protected virtual void OnItemsSuccess(TOutput item)
	{
		_retryStrategy?.Reset();
	}

	protected virtual Task OnItemsError(TOutput item)
	{
		_retryStrategy?.Wait();
		return Task.CompletedTask;
	}
}