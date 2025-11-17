using Microsoft.Extensions.Configuration;

namespace Storm.Api.Notifications.FreeSms;

public static class FreeSmsConfigurationExtensions
{
	public static FreeSmsConfiguration LoadFreeSmsConfiguration(this IConfiguration configuration)
	{
		return new FreeSmsConfiguration()
		{
			User = configuration.GetValue<string>("User")!,
			Password = configuration.GetValue<string>("Password")!
		};
	}
}