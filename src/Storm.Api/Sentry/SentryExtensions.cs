using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Hosting;
using Storm.Api.Extensions;

namespace Storm.Api.Sentry;

public static class SentryExtensions
{
	public static IWebHostBuilder EnableSentryIfNeeded(this IWebHostBuilder builder, Dictionary<string, string> tags)
	{
		string? sentryRelease = Environment.GetEnvironmentVariable("SENTRY_RELEASE");
		string? sentryUrl = Environment.GetEnvironmentVariable("SENTRY_URL");
		if (sentryRelease.IsNullOrWhiteSpace() || sentryUrl.IsNullOrWhiteSpace())
		{
			return builder;
		}

		return builder.UseSentry((context, options) =>
		{
			options.Dsn = sentryUrl;
			options.Release = sentryRelease;

			foreach ((string key, string value) in tags)
			{
				options.DefaultTags.Add(key, value);
			}
		});
	}

	public static void TrackOnSentry(this Exception ex, string? message = null, [CallerFilePath] string? filepath = null, [CallerLineNumber] int line = 0)
	{
		SentryEvent evt = new(ex);
		evt.SetExtra("location", $"{filepath}:{line}");

		if (message != null)
		{
			if (message.Length > 1024 * 50) //limit to 50kB (Sentry message limit is set to 200kB)
			{
				message = message.Substring(0, 1024 * 50);
			}

			evt.Message = message;
		}

		SentrySdk.CaptureEvent(evt);
	}
}