using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Storm.Api.Dtos;

public class LoginResponse
{
	[JsonPropertyName("access_token")]
	[JsonProperty("access_token")]
	public string AccessToken { get; set; } = "";

	[JsonPropertyName("expires_at")]
	[JsonProperty("expires_at")]
	public DateTime ExpiresAt { get; set; }

	[JsonPropertyName("refresh_token")]
	[JsonProperty("refresh_token")]
	public string? RefreshToken { get; set; }

	[JsonPropertyName("csrf_token")]
	[JsonProperty("csrf_token")]
	public string? CsrfToken { get; set; }
}
