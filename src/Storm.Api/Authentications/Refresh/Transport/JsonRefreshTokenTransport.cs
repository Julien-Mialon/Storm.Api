using Storm.Api.Dtos;

namespace Storm.Api.Authentications.Refresh.Transport;

internal class JsonRefreshTokenTransport : IRefreshTokenTransport
{
	public string? ReadToken(RefreshTokenParameter parameter)
	{
		return string.IsNullOrEmpty(parameter.RefreshToken) ? null : parameter.RefreshToken;
	}

	public bool ValidateTransport(string refreshToken)
	{
		return true;
	}

	public void EmitToken(string refreshToken, TimeSpan duration, LoginResponse response)
	{
		response.RefreshToken = refreshToken;
	}

	public void ClearToken()
	{
	}
}
