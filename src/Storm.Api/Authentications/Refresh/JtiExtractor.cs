using System.IdentityModel.Tokens.Jwt;

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
		catch
		{
			// ignored
		}

		return null;
	}
}