using JsonSerializer = System.Text.Json.JsonSerializer;
using Newtonsoft.Json;
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
	public void LoginResponse_Serialize_SystemTextJson_IncludesAllFields()
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
	public void LoginResponse_Serialize_NewtonsoftJson_IncludesAllFields()
	{
		LoginResponse r = new()
		{
			AccessToken = "at",
			ExpiresAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
			RefreshToken = "rt",
			CsrfToken = "cs",
		};
		string json = JsonConvert.SerializeObject(r);
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

	[Fact]
	public void LoginResponse_DeserializeRoundtrip_SystemTextJson()
	{
		DateTime expires = new(2024, 6, 1, 12, 30, 0, DateTimeKind.Utc);
		LoginResponse r = new() { AccessToken = "at", ExpiresAt = expires, RefreshToken = "rt", CsrfToken = "cs" };
		string json = JsonSerializer.Serialize(r);
		LoginResponse? back = JsonSerializer.Deserialize<LoginResponse>(json);
		back.Should().NotBeNull();
		back!.AccessToken.Should().Be("at");
		back.ExpiresAt.Should().Be(expires);
		back.RefreshToken.Should().Be("rt");
		back.CsrfToken.Should().Be("cs");
	}

	[Fact]
	public void LoginResponse_DeserializeRoundtrip_NewtonsoftJson()
	{
		DateTime expires = new(2024, 6, 1, 12, 30, 0, DateTimeKind.Utc);
		LoginResponse r = new() { AccessToken = "at", ExpiresAt = expires, RefreshToken = "rt", CsrfToken = "cs" };
		string json = JsonConvert.SerializeObject(r);
		LoginResponse? back = JsonConvert.DeserializeObject<LoginResponse>(json);
		back.Should().NotBeNull();
		back!.AccessToken.Should().Be("at");
		back.ExpiresAt.ToUniversalTime().Should().Be(expires);
		back.RefreshToken.Should().Be("rt");
		back.CsrfToken.Should().Be("cs");
	}
}
