using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Storm.Api.Databases.Configurations.HighAvailability;
using Storm.Api.Databases.Connections.HighAvailability;
using Storm.Api.Logs;
using Storm.Api.Logs.Extensions;

namespace Storm.Api.Databases.Workers;

/// <summary>
/// Periodically probes every configured replica so the HA manager has an up-to-date topology.
/// Runs on app lifetime via <see cref="IHostedService"/>.
/// </summary>
internal sealed class DatabaseReplicaHealthWorker : BackgroundService
{
	private readonly IDatabaseReplicaManager _manager;
	private readonly HighAvailabilityOptions _options;
	private readonly ILogService? _logService;

	public DatabaseReplicaHealthWorker(
		IDatabaseReplicaManager manager,
		HighAvailabilityOptions options,
		IServiceProvider services)
	{
		_manager = manager;
		_options = options;
		_logService = services.GetService<ILogService>();
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await _manager.ProbeAllAsync(stoppingToken);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception ex)
			{
				_logService?.Error(x => x
					.WriteMessage("Database replica probe round failed")
					.WriteException(ex));
			}

			try
			{
				await Task.Delay(_options.HealthCheckInterval, stoppingToken);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
		}
	}
}
