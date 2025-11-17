using System.Net;

namespace Storm.Api.Notifications.FreeSms;

public class FreeSmsService
{
	private readonly FreeSmsConfiguration _configuration;
	private readonly IHttpClientFactory _httpClientFactory;

	public FreeSmsService(FreeSmsConfiguration configuration, IHttpClientFactory httpClientFactory)
	{
		_configuration = configuration;
		_httpClientFactory = httpClientFactory;
	}

	public async Task SendSms(string message)
	{
		string encodedMessage = WebUtility.UrlEncode(message);

		await _httpClientFactory.CreateClient(nameof(FreeSmsService))
			.GetAsync($"https://smsapi.free-mobile.fr/sendmsg?user={_configuration.User}&pass={_configuration.Password}&msg={encodedMessage}");
	}
}