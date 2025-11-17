namespace Storm.Api.Servers;

public class ServerCultureConfiguration
{
	public required string DefaultCulture { get; init; }

	public required List<string> SupportedCultures { get; init; }
}