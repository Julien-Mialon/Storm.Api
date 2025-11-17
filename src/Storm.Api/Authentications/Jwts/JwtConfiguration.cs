using Microsoft.Extensions.Configuration;

namespace Storm.Api.Authentications.Jwts;

public class JwtConfiguration<TAccount>
{
	public required byte[] Key { get; init; }
	public required string Issuer { get; init; }
	public required string Audience { get; init; }
	public required TimeSpan Duration { get; init; }
}

public static class JwtConfigurationLoader
{
	public static JwtConfiguration<TAccount> LoadJwtConfiguration<TAccount>(this IConfiguration configuration)
	{
		return new()
		{
			Key = Convert.FromBase64String(configuration.GetValue<string>("Key") ?? throw new InvalidOperationException("No value for configuration JWT.Key")),
			Audience = configuration.GetValue<string>("Audience") ?? throw new InvalidOperationException("No value for configuration JWT.Audience"),
			Issuer = configuration.GetValue<string>("Issuer") ?? throw new InvalidOperationException("No value for configuration JWT.Issuer"),
			Duration = TimeSpan.FromSeconds(configuration.GetValue<int?>("Duration") ?? throw new InvalidOperationException("No value for configuration JWT.Duration")),
		};
	}
}