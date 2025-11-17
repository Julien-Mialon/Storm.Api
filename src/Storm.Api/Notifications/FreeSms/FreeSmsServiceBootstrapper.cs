using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Extensions;

namespace Storm.Api.Notifications.FreeSms;

public static class FreeSmsServiceBootstrapper
{
	public static void AddFreeSmsService(this IServiceCollection services, IConfiguration configuration, string configurationSection = "FreeSms")
	{
		configuration.OnSection(configurationSection, section =>
		{
			services.AddSingleton(section.LoadFreeSmsConfiguration());

			services.AddHttpClient(nameof(FreeSmsService), client =>
			{
				client.Timeout = TimeSpan.FromSeconds(30);
			});
			services.AddSingleton<FreeSmsService>();
		});
	}
}