using Storm.Api.Extensions;
using Storm.Api.Logs;
using Storm.Api.Logs.Extensions;

namespace Storm.Api.Workers.HostedServices;

public abstract class BaseTimeRunHostedService : BaseHostedService
{
	private readonly TimeOnly[] _runTimes;

	public BaseTimeRunHostedService(IServiceProvider services, params TimeOnly[] runTimes) : base(services)
	{
		_runTimes = runTimes;
	}

	protected abstract Task Run(IServiceProvider services);

	protected virtual void OnException(Exception ex)
	{
		Resolve<ILogService>().Critical(x => x
			.WriteMessage($"{GetType().Name}: exception in hosted service")
			.WriteException(ex));
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (true)
		{
			if (stoppingToken.IsCancellationRequested)
			{
				break;
			}

			try
			{
				await AwaitNextRun(stoppingToken);

				await Services.ExecuteWithScope(async scope =>
				{
					await Run(scope);
				});
			}
			catch (Exception ex)
			{
				OnException(ex);
			}
		}
	}

	private async Task AwaitNextRun(CancellationToken token)
	{
		DateTime now = DateTime.UtcNow;
		DateTime waitDate = _runTimes.Select(x =>
		{
			DateTime r = new(now.Year, now.Month, now.Day, x.Hour, x.Minute, x.Second);
			if (r < now)
			{
				return r.AddDays(1);
			}

			return r;
		}).MinBy(x => x);

		TimeSpan waitTime = waitDate - now;
		if (waitTime.TotalSeconds > 0)
		{
			await Task.Delay(waitTime, token);
		}
	}
}