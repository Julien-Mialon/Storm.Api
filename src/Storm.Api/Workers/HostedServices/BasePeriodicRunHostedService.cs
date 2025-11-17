using Storm.Api.Extensions;
using Storm.Api.Logs;
using Storm.Api.Logs.Extensions;

namespace Storm.Api.Workers.HostedServices;

public abstract class BasePeriodicRunHostedService : BaseHostedService
{
	private readonly TimeSpan _interval;

	protected BasePeriodicRunHostedService(IServiceProvider services, TimeSpan interval) : base(services)
	{
		_interval = interval;
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
				await Services.ExecuteWithScope(async scope =>
				{
					await Run(scope);
				});

				await Task.Delay(_interval, stoppingToken);
			}
			catch (Exception ex)
			{
				OnException(ex);
			}
		}
	}
}