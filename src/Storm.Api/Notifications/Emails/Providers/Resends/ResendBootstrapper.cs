using Microsoft.Extensions.DependencyInjection;
using Resend;

namespace Storm.Api.Notifications.Emails.Providers.Resends;

public static class ResendBootstrapper
{
	public static IServiceCollection AddResend(this IServiceCollection services, ResendConfiguration configuration)
	{
		services.AddTransient<IResend>(s => ResendClient.Create(configuration.ApiKey))
			.AddTransient<IEmailService, ResendEmailService>()
			.AddHttpClient<ResendClient>();

		return services;
	}
}