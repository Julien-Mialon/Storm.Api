using Storm.Api.Logs;
using Storm.Api.Logs.Extensions;

namespace Storm.Api.Workers;

public class BackgroundWorker
{
	private readonly Lock _mutex = new();
	private readonly ILogService _logService;
	private readonly Func<CancellationToken, Task> _run;
	private Task? _currentTask;
	private CancellationTokenSource? _cts;

	public BackgroundWorker(ILogService logService, Func<CancellationToken, Task> run)
	{
		_logService = logService;
		_run = run;
	}

	public void Start()
	{
		if (_currentTask == null || _cts == null || _cts.IsCancellationRequested || _currentTask.IsCompleted)
		{
			lock (_mutex)
			{
				if (_currentTask == null || _cts == null || _cts.IsCancellationRequested || _currentTask.IsCompleted)
				{
					_cts?.Dispose();
					_cts = new();
					_currentTask = Task.Run(() => Run(_cts.Token), _cts.Token);
				}
			}
		}
	}

	public void Stop()
	{
		lock (_mutex)
		{
			_cts?.Cancel();
			_currentTask = null;
		}
	}

	private async Task Run(CancellationToken ct)
	{
		try
		{
			_logService.Trace("BackgroundWorker started");
			await _run(ct);
			_logService.Trace("BackgroundWorker finished");
		}
		catch (Exception ex)
		{
			_logService.Error(x => x
				.WriteMessage("Background worker stopped with exception")
				.WriteException(ex));
		}
	}
}