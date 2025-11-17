using Resend;

namespace Storm.Api.Notifications.Emails.Providers.Resends;

public class ResendEmailService : IEmailService
{
	private readonly IResend _resend;

	public ResendEmailService(IResend resend)
	{
		_resend = resend;
	}

	public Task Send(EmailContent content)
	{
		EmailMessage message = new()
		{
			From = content.From,
			Subject = content.Subject,
			HtmlBody = content.ContentHtml,
			TextBody = content.ContentText,
		};

		content.To.ForEach(x => message.To.Add(x));
		if (content.Cc.Count > 0)
		{
			message.Cc ??= [];
			content.Cc.ForEach(x => message.Cc.Add(x));
		}

		if (content.Bcc.Count > 0)
		{
			message.Bcc ??= [];
			content.Bcc.ForEach(x => message.Bcc.Add(x));
		}

		return _resend.EmailSendAsync(message);
	}
}