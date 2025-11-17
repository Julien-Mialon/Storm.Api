using Microsoft.Extensions.Configuration;
using Storm.Api.Extensions;

namespace Storm.Api.Servers;

public class ServerConfiguration
{
	public bool ForceHttps { get; init; }

	public required ServerCultureConfiguration Cultures { get; init; }

	public required ServerFormConfiguration Forms { get; init; }
}

public static class ServerConfigurationExtensions
{
	private static readonly ServerFormConfiguration DEFAULT_SERVER_FORM_CONFIGURATION = new()
	{
		ValueLengthLimit = 1024 * 1024 * 1024,
		MultipartBodyLengthLimit = 1024 * 1024 * 1024,
	};

	private static readonly ServerCultureConfiguration DEFAULT_SERVER_CULTURE_CONFIGURATION = new()
	{
		DefaultCulture = "fr",
		SupportedCultures = new() { "fr" },
	};

	public static readonly ServerConfiguration DEFAULT_SERVER_CONFIGURATION = new()
	{
		ForceHttps = true,
		Forms = DEFAULT_SERVER_FORM_CONFIGURATION,
		Cultures = DEFAULT_SERVER_CULTURE_CONFIGURATION,
	};

	public static ServerConfiguration LoadServerConfiguration(this IConfiguration configuration)
	{
		return new()
		{
			ForceHttps = configuration.GetValue("ForceHttps", true),
			Forms = configuration.WithSection("Forms", LoadServerFormConfiguration) ?? DEFAULT_SERVER_FORM_CONFIGURATION,
			Cultures = configuration.WithSection("Cultures", LoadServerCultureConfiguration) ?? DEFAULT_SERVER_CULTURE_CONFIGURATION,
		};
	}

	private static ServerCultureConfiguration LoadServerCultureConfiguration(this IConfiguration configuration)
	{
		string[]? supportedCultures = null;
		if (configuration.GetValue<string>("Supported") is { } multiKeysStrings)
		{
			supportedCultures = multiKeysStrings.Split(new[]
			{
				',',
				';'
			}, StringSplitOptions.RemoveEmptyEntries);
		}
		else
		{
			supportedCultures = configuration.GetSection("Supported").Get<string[]>();
		}

		return new()
		{
			DefaultCulture = configuration.GetValue<string>("Default").ValueIfNullOrEmpty(DEFAULT_SERVER_CULTURE_CONFIGURATION.DefaultCulture),
			SupportedCultures = supportedCultures?.ToList() ?? DEFAULT_SERVER_CULTURE_CONFIGURATION.SupportedCultures
		};
	}

	private static ServerFormConfiguration LoadServerFormConfiguration(this IConfiguration configuration)
	{
		return new()
		{
			ValueLengthLimit = configuration.GetValue("ValueLengthLimit", DEFAULT_SERVER_FORM_CONFIGURATION.ValueLengthLimit),
			MultipartBodyLengthLimit = configuration.GetValue("MultipartBodyLengthLimit", DEFAULT_SERVER_FORM_CONFIGURATION.MultipartBodyLengthLimit),
		};
	}
}