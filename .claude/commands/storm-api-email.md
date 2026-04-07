You are helping implement email sending using the **Storm.Api** framework. Follow all patterns below exactly. For global rules (logging, extensions, anti-patterns), see `/storm-api`.

The user's request: $ARGUMENTS

---

## How Email Works

Storm.Api provides an `IEmailService` interface with a single method `Send(EmailContent)`. The only built-in provider is **Resend**. You inject `IEmailService` into actions via `Resolve<IEmailService>()` and call `Send()` with an `EmailContent` object.

---

## Configuration

### appsettings.json

```json
{
  "Resend": {
    "ApiKey": "<your-resend-api-key>"
  }
}
```

### Register in Startup.ConfigureServices

```csharp
services.AddResend(
    configuration.GetSection("Resend").LoadResendConfiguration()
);
```

This registers:
- `IResend` (the Resend SDK client)
- `IEmailService` as `ResendEmailService`

**Required using statements:**
```csharp
using Storm.Api.Notifications.Emails.Providers.Resends;
```

---

## Sending an Email

Use `IEmailService` inside any action or service that has access to `Resolve<T>()`:

```csharp
var emailService = Resolve<IEmailService>();

await emailService.Send(new EmailContent
{
    From = "noreply@example.com",
    To = ["user@example.com"],
    Subject = "Welcome!",
    ContentHtml = "<h1>Hello</h1><p>Welcome to our app.</p>",
    ContentText = "Hello\nWelcome to our app.",
});
```

### EmailContent Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `From` | `string` | Yes | Sender address (must be verified in Resend) |
| `To` | `List<string>` | Yes | Recipient addresses |
| `Cc` | `List<string>` | No | CC addresses |
| `Bcc` | `List<string>` | No | BCC addresses |
| `Subject` | `string` | Yes | Email subject line |
| `ContentHtml` | `string?` | No | HTML body (at least one of Html/Text should be set) |
| `ContentText` | `string?` | No | Plain text body |

**Required using statements:**
```csharp
using Storm.Api.Notifications.Emails;
```

---

## Temporary Email Detection

Storm.Api includes a helper to detect disposable/temporary email addresses. Use it to validate user emails during registration or similar flows:

```csharp
using Storm.Api.Helpers;

bool isDisposable = userEmail.IsTemporaryEmailDomain();
if (isDisposable)
{
    throw new DomainException("DISPOSABLE_EMAIL", "Disposable email addresses are not allowed.");
}
```

`IsTemporaryEmailDomain()` is an extension method on `string` that checks the email's domain against a built-in blocklist of known disposable email providers.

---

## Full Example: Action That Sends Email

```csharp
using Storm.Api.CQRS;
using Storm.Api.Notifications.Emails;

public class SendWelcomeEmailCommand(IServiceProvider services)
    : BaseAction<SendWelcomeEmailParameter, Unit>(services)
{
    protected override async Task<Unit> Action(SendWelcomeEmailParameter parameter)
    {
        var emailService = Resolve<IEmailService>();

        await emailService.Send(new EmailContent
        {
            From = "noreply@example.com",
            To = [parameter.Email],
            Subject = "Welcome!",
            ContentHtml = $"<p>Welcome, {parameter.Name}!</p>",
        });

        return Unit.Default;
    }
}
```

---

## Anti-Patterns to Avoid

| ❌ Wrong | ✅ Correct |
|---|---|
| Instantiate `ResendClient` directly | Use `IEmailService` via DI |
| Use `SmtpClient` or other mail libraries | Use `IEmailService` with Resend provider |
| Skip email validation on user input | Use `IsTemporaryEmailDomain()` to reject disposable emails |
