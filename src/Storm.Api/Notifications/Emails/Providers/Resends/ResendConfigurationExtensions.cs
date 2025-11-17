using Microsoft.Extensions.Configuration;

namespace Storm.Api.Notifications.Emails.Providers.Resends;

public static class ResendConfigurationExtensions
{
	public static ResendConfiguration LoadResendConfiguration(this IConfiguration configuration)
	{
		return new()
		{
			ApiKey = configuration.GetValue<string>("ApiKey") ?? throw new InvalidOperationException("No value for configuration Resend.ApiKey")
		};
	}
}