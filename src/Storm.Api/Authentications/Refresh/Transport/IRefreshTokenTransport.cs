using Storm.Api.Dtos;

namespace Storm.Api.Authentications.Refresh.Transport;

public interface IRefreshTokenTransport
{
	string? ReadToken(RefreshTokenParameter parameter);

	bool ValidateTransport(string refreshToken);

	void EmitToken(string refreshToken, TimeSpan duration, LoginResponse response);

	void ClearToken();
}