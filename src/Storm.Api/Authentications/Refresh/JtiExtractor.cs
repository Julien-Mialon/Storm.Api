using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace Storm.Api.Authentications.Refresh;

internal static class JtiExtractor
{
	public static string? Extract(string token)
	{
		try
		{
			JwtSecurityTokenHandler handler = new();
			if (handler.ReadToken(token) is JwtSecurityToken jwtToken)
			{
				return jwtToken.Claims.FirstOrDefault(x => x.Type == "jti")?.Value;
			}
		}
		catch (Exception ex) when (ex is ArgumentException or SecurityTokenException)
		{
			// Malformed or invalid token — treat as missing JTI
		}

		return null;
	}
}
