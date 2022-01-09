using Storm.Api.Core.Logs;

namespace Storm.Api.Core.Workers;

public class BackgroundItemExponentialQueueWorker<TWorkItem> : BackgroundItemWorker<TWorkItem>
	where TWorkItem : class
{
	private readonly ExponentialBackOffStrategy _backOffStrategy;

	public BackgroundItemExponentialQueueWorker(ILogService logService, Func<TWorkItem, Task<bool>> itemAction, Action<TWorkItem?, Exception>? onException = null, int? discardAfterFailAttemptsCount = null) : base(logService, itemAction, onException, discardAfterFailAttemptsCount)
	{
		_backOffStrategy = new(5000, 4);
	}

	protected override void OnItemSuccess(TWorkItem item)
	{
		base.OnItemSuccess(item);

		_backOffStrategy.Reset();
	}

	protected override async Task OnItemError(TWorkItem item)
	{
		await base.OnItemError(item);

		await _backOffStrategy.Wait();
	}
}