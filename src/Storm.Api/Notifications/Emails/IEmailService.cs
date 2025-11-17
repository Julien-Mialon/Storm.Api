namespace Storm.Api.Notifications.Emails;

public interface IEmailService
{
	Task Send(EmailContent content);
}