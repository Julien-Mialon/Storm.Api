using Storm.Api.Authentications.Commons;
using Storm.Api.Extensions;

namespace Storm.Api.Authentications.Jwts;

public class JwtRawAuthenticator : BaseTokenAuthenticator<Guid?>
{
	public JwtRawAuthenticator(IServiceProvider services, string? headerName, string? queryParameterName, string? tokenType) : base(services, headerName, queryParameterName, tokenType)
	{
		
	}

	protected override Task<Guid?> Authenticate(string token)
	{
		if (Resolve<JwtService<Guid>>().TryValidateToken(token, out Guid userId))
		{
			return userId.AsTaskNullable();
		}

		return ((Guid?)null).AsTaskNullable();
	}
}