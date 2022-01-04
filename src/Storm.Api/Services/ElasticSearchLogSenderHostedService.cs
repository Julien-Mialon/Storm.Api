using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Storm.Api.Core.Logs;
using Storm.Api.Core.Logs.ElasticSearch.Senders;

namespace Storm.Api.Services;

public class ElasticSearchLogSenderHostedService : BackgroundService
{
	private const int BULK_SIZE = 50;
	private readonly IServiceProvider _services;
	private readonly TimeSpan _timeoutBeforeSend = TimeSpan.FromSeconds(10);

	public ElasticSearchLogSenderHostedService(IServiceProvider services)
	{
		_services = services;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		ILogQueueService logQueueService = _services.GetRequiredService<ILogQueueService>();
		IElasticSender sender = _services.GetRequiredService<IElasticSender>();
		List<string> items = new List<string>(BULK_SIZE);
		while (true)
		{
			try
			{
				if (stoppingToken.IsCancellationRequested)
				{
					return;
				}

				for (int i = 0; i < BULK_SIZE; ++i)
				{
					string? item = await logQueueService.Next(_timeoutBeforeSend);
					if (item is null)
					{
						break;
					}

					items.Add(item);
				}

				if (items.Count > 0)
				{
					await sender.Send(items);
					items.Clear();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}
}