using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Logs;
using Storm.Api.Logs.Extensions;

namespace Storm.Api.Workers.HostedServices;

public class AppMetricsHostedService : BasePeriodicRunHostedService
{
	public AppMetricsHostedService(IServiceProvider services) : base(services, TimeSpan.FromSeconds(5))
	{
	}

	protected override Task Run(IServiceProvider services)
	{
		services.GetRequiredService<ILogService>().Information(x => x
			.WriteProperty("filter", "metrics")
			.WriteObject("metrics", y => y
				.WriteProperty("thread_count", ThreadPool.ThreadCount)
				.WriteProperty("completed_work_item_count", ThreadPool.CompletedWorkItemCount)
				.WriteProperty("pending_work_item_count", ThreadPool.PendingWorkItemCount)
				.WriteProperty("gc_total_memory", GC.GetTotalMemory(false))
				.WriteProperty("gc_total_allocated_bytes", GC.GetTotalAllocatedBytes(false))
			)
		);

		return Task.CompletedTask;
	}
}