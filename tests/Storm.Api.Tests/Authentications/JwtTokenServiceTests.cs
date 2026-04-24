using System.IdentityModel.Tokens.Jwt;
using Storm.Api.Authentications.Jwts;

namespace Storm.Api.Tests.Authentications;

public class JwtTokenServiceTests
{
	private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
	{
		public override DateTimeOffset GetUtcNow() => now;
	}

	private static readonly DateTimeOffset DefaultNow = DateTimeOffset.UtcNow;

	private static IJwtTokenService MakeService(DateTimeOffset? now = null)
	{
		Type? t = typeof(IJwtTokenService).Assembly.GetType("Storm.Api.Authentications.Jwts.JwtTokenService");
		TimeProvider tp = new FixedTimeProvider(now ?? DefaultNow);
		return (IJwtTokenService)Activator.CreateInstance(t!, tp)!;
	}

	private const string Aud = "aud";
	private const string Iss = "iss";
	private static readonly byte[] Key = System.Text.Encoding.UTF8.GetBytes("0123456789012345678901234567890123456789");

	[Fact]
	public void GenerateToken_IncludesSubjectClaim()
	{
		IJwtTokenService svc = MakeService();
		string token = svc.GenerateToken("user-1", Aud, Iss, TimeSpan.FromHours(1), Key);
		JwtSecurityToken parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
		parsed.Claims.Should().Contain(c => c.Type == "id" && c.Value == "user-1");
	}

	[Fact]
	public void GenerateToken_IncludesAudienceAndIssuer()
	{
		IJwtTokenService svc = MakeService();
		string token = svc.GenerateToken("u", Aud, Iss, TimeSpan.FromHours(1), Key);
		JwtSecurityToken parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
		parsed.Audiences.Should().Contain(Aud);
		parsed.Issuer.Should().Be(Iss);
	}

	[Fact]
	public void GenerateToken_IncludesExpirationBasedOnTimeProvider()
	{
		DateTimeOffset now = DefaultNow.AddYears(5);
		IJwtTokenService svc = MakeService(now);
		string token = svc.GenerateToken("u", Aud, Iss, TimeSpan.FromHours(1), Key);
		JwtSecurityToken parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
		parsed.ValidTo.Should().BeCloseTo(now.UtcDateTime.AddHours(1), TimeSpan.FromSeconds(2));
	}

	[Fact]
	public void GenerateToken_IncludesAdditionalClaims_WhenProvided()
	{
		IJwtTokenService svc = MakeService();
		IReadOnlyDictionary<string, string> extra = new Dictionary<string, string> { ["role"] = "admin" };
		string token = svc.GenerateToken("u", Aud, Iss, TimeSpan.FromHours(1), Key, extra);
		JwtSecurityToken parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);
		parsed.Claims.Should().Contain(c => c.Type == "role" && c.Value == "admin");
	}

	[Fact]
	public void TryGetIdWithoutValidation_ReturnsClaimIdEvenIfTampered()
	{
		IJwtTokenService svc = MakeService();
		string token = svc.GenerateToken("u", Aud, Iss, TimeSpan.FromHours(1), Key);
		string tampered = token[..^5] + "XXXXX";
		svc.TryGetIdWithoutValidation(tampered, out string? id).Should().BeTrue();
		id.Should().Be("u");
	}

	[Fact]
	public void TryGetId_ValidToken_ReturnsId()
	{
		IJwtTokenService svc = MakeService();
		string token = svc.GenerateToken("u", Aud, Iss, TimeSpan.FromHours(1), Key);
		svc.TryGetId(token, Aud, Iss, Key, out string? id).Should().BeTrue();
		id.Should().Be("u");
	}

	[Fact]
	public void TryGetId_ValidToken_Short_ReturnsId()
	{
		IJwtTokenService svc = MakeService();
		string token = svc.GenerateToken("u", Aud, Iss, TimeSpan.FromMinutes(2), Key);
		svc.TryGetId(token, Aud, Iss, Key, out string? id).Should().BeTrue();
		id.Should().Be("u");
	}

	[Fact]
	public void TryGetId_TamperedSignature_ReturnsFalse()
	{
		IJwtTokenService svc = MakeService();
		string token = svc.GenerateToken("u", Aud, Iss, TimeSpan.FromHours(1), Key);
		string tampered = token[..^5] + "XXXXX";
		svc.TryGetId(tampered, Aud, Iss, Key, out string? _).Should().BeFalse();
	}

	[Fact]
	public void TryGetId_ExpiredToken_WithZeroClockSkew_ReturnsFalse()
	{
		DateTimeOffset issued = DefaultNow;
		IJwtTokenService issuer = MakeService(issued);
		string token = issuer.GenerateToken("u", Aud, Iss, TimeSpan.FromMinutes(1), Key);

		// validator sees time 2 min after issue → token expired
		IJwtTokenService validator = MakeService(issued.AddMinutes(2));
		validator.TryGetId(token, Aud, Iss, Key, out string? _, TimeSpan.Zero).Should().BeFalse();
	}

	[Fact]
	public void TryGetId_ExpiredToken_WithinClockSkew_ReturnsId()
	{
		DateTimeOffset issued = DefaultNow;
		IJwtTokenService issuer = MakeService(issued);
		string token = issuer.GenerateToken("u", Aud, Iss, TimeSpan.FromMinutes(1), Key);

		// token expired 30s ago from validator's point of view, 2-min skew accepts it
		IJwtTokenService validator = MakeService(issued.AddMinutes(1).AddSeconds(30));
		validator.TryGetId(token, Aud, Iss, Key, out string? id, TimeSpan.FromMinutes(2)).Should().BeTrue();
		id.Should().Be("u");
	}

	[Fact]
	public void TryGetId_ExpiredToken_DefaultClockSkew_ReturnsId()
	{
		DateTimeOffset issued = DefaultNow;
		IJwtTokenService issuer = MakeService(issued);
		string token = issuer.GenerateToken("u", Aud, Iss, TimeSpan.FromMinutes(1), Key);

		// token expired 1min ago; default 5-min skew accepts it
		IJwtTokenService validator = MakeService(issued.AddMinutes(2));
		validator.TryGetId(token, Aud, Iss, Key, out string? id).Should().BeTrue();
		id.Should().Be("u");
	}

	[Fact]
	public void TryGetId_ExpiredToken_BeyondDefaultClockSkew_ReturnsFalse()
	{
		DateTimeOffset issued = DefaultNow;
		IJwtTokenService issuer = MakeService(issued);
		string token = issuer.GenerateToken("u", Aud, Iss, TimeSpan.FromMinutes(1), Key);

		// token expired 10min ago; default 5-min skew rejects it
		IJwtTokenService validator = MakeService(issued.AddMinutes(11));
		validator.TryGetId(token, Aud, Iss, Key, out string? _).Should().BeFalse();
	}

	[Fact]
	public void TryGetId_NotYetValid_ReturnsFalse()
	{
		// token issued "in the future" from validator's perspective
		DateTimeOffset issued = DefaultNow.AddHours(2);
		IJwtTokenService issuer = MakeService(issued);
		string token = issuer.GenerateToken("u", Aud, Iss, TimeSpan.FromMinutes(1), Key);

		IJwtTokenService validator = MakeService(DefaultNow);
		validator.TryGetId(token, Aud, Iss, Key, out string? _, TimeSpan.Zero).Should().BeFalse();
	}

	[Fact]
	public void TryGetId_WrongAudience_ReturnsFalse()
	{
		IJwtTokenService svc = MakeService();
		string token = svc.GenerateToken("u", Aud, Iss, TimeSpan.FromHours(1), Key);
		svc.TryGetId(token, "other-aud", Iss, Key, out string? _).Should().BeFalse();
	}

	[Fact]
	public void TryGetId_WrongIssuer_ReturnsFalse()
	{
		IJwtTokenService svc = MakeService();
		string token = svc.GenerateToken("u", Aud, Iss, TimeSpan.FromHours(1), Key);
		svc.TryGetId(token, Aud, "other-iss", Key, out string? _).Should().BeFalse();
	}

	[Fact]
	public void TryGetId_Malformed_ReturnsFalse()
	{
		IJwtTokenService svc = MakeService();
		svc.TryGetId("not.a.token", Aud, Iss, Key, out string? _).Should().BeFalse();
	}

	[Fact]
	public void IsValid_WrapsTryGetId()
	{
		IJwtTokenService svc = MakeService();
		string token = svc.GenerateToken("u", Aud, Iss, TimeSpan.FromHours(1), Key);
		svc.IsValid(token, Aud, Iss, Key).Should().BeTrue();
		svc.IsValid("bad", Aud, Iss, Key).Should().BeFalse();
	}

	[Fact]
	public void Guid_Id_RoundtripsCorrectly()
	{
		IJwtTokenService svc = MakeService();
		Guid id = Guid.NewGuid();
		string token = svc.GenerateToken(id, Aud, Iss, TimeSpan.FromHours(1), Key);
		svc.TryGetId(token, Aud, Iss, Key, out Guid roundtrip).Should().BeTrue();
		roundtrip.Should().Be(id);
	}
}
