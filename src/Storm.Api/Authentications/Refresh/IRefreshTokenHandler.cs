using Storm.Api.Dtos;

namespace Storm.Api.Authentications.Refresh;

public interface IRefreshTokenHandler
{
	/// <summary>
	/// Reads the inbound refresh token from the request.
	/// Cookie mode reads from HttpContext cookies (ignores parameter).
	/// Database mode reads from the parameter body.
	/// </summary>
	string? ReadInboundToken(RefreshTokenParameter parameter);

	/// <summary>
	/// Performs transport-specific validation before JWT validation.
	/// Cookie mode validates the CSRF header. Database mode returns true.
	/// </summary>
	bool ValidateTransport(string refreshToken);

	/// <summary>
	/// Validates token state after JWT signature is verified.
	/// Cookie mode returns true (stateless). Database mode checks JTI exists in the store.
	/// </summary>
	Task<bool> ValidateTokenStateAsync(string refreshToken, string jti);

	/// <summary>
	/// Stores a newly issued refresh token and populates the response with transport-specific fields.
	/// Cookie mode sets the HttpOnly cookie and populates <see cref="LoginResponse.CsrfToken" />.
	/// Database mode persists the JTI and populates <see cref="LoginResponse.RefreshToken" />.
	/// </summary>
	Task StoreAndEmitAsync(Guid accountId, string refreshToken, string jti, TimeSpan duration, LoginResponse response);

	/// <summary>
	/// Revokes or clears the refresh token.
	/// Cookie mode clears the cookie. Database mode deletes the JTI from the store.
	/// </summary>
	Task RevokeAsync(string? refreshToken, string? jti);
}