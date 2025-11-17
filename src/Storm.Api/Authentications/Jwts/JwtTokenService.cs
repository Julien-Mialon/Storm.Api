using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Storm.Api.Authentications.Jwts;

public interface IJwtTokenService
{
	string GenerateToken(Guid idClaim, string audience, string issuer, TimeSpan duration, byte[] signatureKey);
	string GenerateToken(string idClaim, string audience, string issuer, TimeSpan duration, byte[] signatureKey);
	bool TryGetIdWithoutValidation(string token, [NotNullWhen(true)] out string? idClaim);
	bool TryGetIdWithoutValidation(string token, out Guid idClaim);
	bool TryGetId(string token, string audience, string issuer, byte[] signatureKey, [NotNullWhen(true)] out string? idClaim);
	bool TryGetId(string token, string audience, string issuer, byte[] signatureKey, out Guid idClaim);
	bool IsValid(string token, string audience, string issuer, byte[] signatureKey);
}

internal class JwtTokenService : IJwtTokenService
{
	public string GenerateToken(Guid idClaim, string audience, string issuer, TimeSpan duration, byte[] signatureKey)
	{
		return GenerateToken(idClaim.ToString("N"), audience, issuer, duration, signatureKey);
	}

	public string GenerateToken(string idClaim, string audience, string issuer, TimeSpan duration, byte[] signatureKey)
	{
		JwtSecurityTokenHandler handler = new();
		SecurityTokenDescriptor descriptor = new()
		{
			Subject = new([
				new Claim("id", idClaim)
			]),
			Audience = audience,
			Issuer = issuer,
			Expires = DateTime.UtcNow.Add(duration),
			SigningCredentials = new(new SymmetricSecurityKey(signatureKey), SecurityAlgorithms.HmacSha256)
		};

		SecurityToken? token = handler.CreateToken(descriptor);
		string accessToken = handler.WriteToken(token);

		return accessToken;
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

	public bool TryGetId(string token, string audience, string issuer, byte[] signatureKey, [NotNullWhen(true)] out string? idClaim)
	{
		try
		{
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

	public bool TryGetId(string token, string audience, string issuer, byte[] signatureKey, out Guid idClaim)
	{
		if (TryGetId(token, audience, issuer, signatureKey, out string? idClaimString) && Guid.TryParse(idClaimString, out idClaim))
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
}