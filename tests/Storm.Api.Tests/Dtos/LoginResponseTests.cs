using System.Text.Json;
using Storm.Api.Dtos;

namespace Storm.Api.Tests.Dtos;

public class LoginResponseTests
{
	[Fact]
	public void LoginResponse_Default_AccessTokenIsEmptyString()
	{
		new LoginResponse().AccessToken.Should().Be("");
	}

	[Fact]
	public void LoginResponse_Serialize_IncludesAllFields()
	{
		LoginResponse r = new()
		{
			AccessToken = "at",
			ExpiresAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			RefreshToken = "rt",
			CsrfToken = "cs",
		};
		string json = JsonSerializer.Serialize(r);
		json.Should().Contain("\"access_token\":\"at\"");
		json.Should().Contain("\"expires_at\":");
		json.Should().Contain("\"refresh_token\":\"rt\"");
		json.Should().Contain("\"csrf_token\":\"cs\"");
	}

	[Fact]
	public void LoginResponse_SerializeNullRefreshToken_EmitsNull()
	{
		LoginResponse r = new() { RefreshToken = null };
		string json = JsonSerializer.Serialize(r);
		json.Should().Contain("\"refresh_token\":null");
	}
}
