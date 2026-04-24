using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Storm.Api.Extensions;

namespace Storm.Api.Authentications.Jwts;

public interface IJwtTokenService
{
	string GenerateToken(Guid idClaim, string audience, string issuer, TimeSpan duration, byte[] signatureKey);

	string GenerateToken(string idClaim, string audience, string issuer, TimeSpan duration, byte[] signatureKey);

	string GenerateToken(Guid idClaim, string audience, string issuer, TimeSpan duration, byte[] signatureKey, IReadOnlyDictionary<string, string>? additionalClaims);

	string GenerateToken(string idClaim, string audience, string issuer, TimeSpan duration, byte[] signatureKey, IReadOnlyDictionary<string, string>? additionalClaims);

	bool TryGetIdWithoutValidation(string token, [NotNullWhen(true)] out string? idClaim);

	bool TryGetIdWithoutValidation(string token, out Guid idClaim);

	bool TryGetId(string token, string audience, string issuer, byte[] signatureKey, [NotNullWhen(true)] out string? idClaim, TimeSpan? clockSkew = null);

	bool TryGetId(string token, string audience, string issuer, byte[] signatureKey, out Guid idClaim, TimeSpan? clockSkew = null);

	bool IsValid(string token, string audience, string issuer, byte[] signatureKey);
}

internal class JwtTokenService(TimeProvider timeProvider) : IJwtTokenService
{
	public string GenerateToken(Guid idClaim, string audience, string issuer, TimeSpan duration, byte[] signatureKey)
	{
		return GenerateToken(idClaim.ToString("N"), audience, issuer, duration, signatureKey, null);
	}

	public string GenerateToken(string idClaim, string audience, string issuer, TimeSpan duration, byte[] signatureKey)
	{
		return GenerateToken(idClaim, audience, issuer, duration, signatureKey, null);
	}

	public string GenerateToken(Guid idClaim, string audience, string issuer, TimeSpan duration, byte[] signatureKey, IReadOnlyDictionary<string, string>? additionalClaims)
	{
		return GenerateToken(idClaim.ToString("N"), audience, issuer, duration, signatureKey, additionalClaims);
	}

	public string GenerateToken(string idClaim, string audience, string issuer, TimeSpan duration, byte[] signatureKey, IReadOnlyDictionary<string, string>? additionalClaims)
	{
		JwtSecurityTokenHandler handler = new();

		List<Claim> claims = [new Claim("id", idClaim)];
		if (additionalClaims is not null)
		{
			foreach (KeyValuePair<string, string> entry in additionalClaims)
			{
				claims.Add(new Claim(entry.Key, entry.Value));
			}
		}

		DateTime now = timeProvider.GetUtcNow().UtcDateTime;
		SecurityTokenDescriptor descriptor = new()
		{
			Subject = new ClaimsIdentity(claims),
			Audience = audience,
			Issuer = issuer,
			Expires = now.Add(duration),
			SigningCredentials = new(new SymmetricSecurityKey(signatureKey), SecurityAlgorithms.HmacSha256),
			IssuedAt = now,
			NotBefore = now,
		};

		SecurityToken? token = handler.CreateToken(descriptor);
		return handler.WriteToken(token);
	}

	public bool TryGetIdWithoutValidation(string token, [NotNullWhen(true)] out string? idClaim)
	{
		try
		{
			JwtSecurityTokenHandler handler = new();

			if (handler.ReadToken(token) is JwtSecurityToken jwtToken)
			{
				Claim? claim = jwtToken.Claims.FirstOrDefault(x => x.Type == "id");
				if (claim is not null)
				{
					idClaim = claim.Value;
					return true;
				}
			}
		}
		catch
		{
			// ignored
		}

		idClaim = null;
		return false;
	}

	public bool TryGetIdWithoutValidation(string token, out Guid idClaim)
	{
		if (TryGetIdWithoutValidation(token, out string? idClaimString) && Guid.TryParse(idClaimString, out idClaim))
		{
			return true;
		}

		idClaim = Guid.Empty;
		return false;
	}

	public bool TryGetId(string token, string audience, string issuer, byte[] signatureKey, [NotNullWhen(true)] out string? idClaim, TimeSpan? clockSkew = null)
	{
		try
		{
			TimeSpan skew = clockSkew ?? TimeSpan.FromMinutes(5);
			JwtSecurityTokenHandler handler = new();
			handler.ValidateToken(token, new()
			{
				ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(signatureKey),
				ValidateIssuer = true,
				ValidIssuer = issuer,
				ValidateAudience = true,
				ValidAudience = audience,
				RequireSignedTokens = true,
				RequireAudience = true,
				RequireExpirationTime = true,
				ValidateLifetime = true,
				ClockSkew = skew,
				LifetimeValidator = (notBefore, expires, _, _) => ValidateLifetime(notBefore, expires, skew),
			}, out SecurityToken validToken);

			if (validToken is JwtSecurityToken jwtToken)
			{
				Claim? claim = jwtToken.Claims.FirstOrDefault(x => x.Type == "id");
				if (claim is not null)
				{
					idClaim = claim.Value;
					return true;
				}
			}
		}
		catch
		{
			// ignored
		}

		idClaim = null;
		return false;
	}

	public bool TryGetId(string token, string audience, string issuer, byte[] signatureKey, out Guid idClaim, TimeSpan? clockSkew = null)
	{
		if (TryGetId(token, audience, issuer, signatureKey, out string? idClaimString, clockSkew) && Guid.TryParse(idClaimString, out idClaim))
		{
			return true;
		}

		idClaim = Guid.Empty;
		return false;
	}

	public bool IsValid(string token, string audience, string issuer, byte[] signatureKey)
	{
		return TryGetId(token, audience, issuer, signatureKey, out string? _);
	}

	private bool ValidateLifetime(DateTime? notBefore, DateTime? expires, TimeSpan clockSkew)
	{
		if (expires is null)
		{
			return false;
		}

		if (notBefore is not null && notBefore.Value.Subtract(clockSkew).IsFuture(timeProvider))
		{
			return false;
		}

		if (expires.Value.Add(clockSkew).IsPast(timeProvider))
		{
			return false;
		}

		return true;
	}
}