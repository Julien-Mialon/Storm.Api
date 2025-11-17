namespace Storm.Api.Notifications.Emails;

public class EmailContent
{
	public string From { get; set; } = "";

	public List<string> To { get; set; } = [];

	public List<string> Cc { get; set; } = [];

	public List<string> Bcc { get; set; } = [];

	public string Subject { get; set; } = "";

	public string? ContentHtml { get; set; }

	public string? ContentText { get; set; }
}