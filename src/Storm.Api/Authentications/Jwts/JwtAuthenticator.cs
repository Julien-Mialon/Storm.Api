using Storm.Api.Authentications.Commons;
using Storm.Api.Databases.Repositories;

namespace Storm.Api.Authentications.Jwts;

public class JwtAuthenticator<TAccount> : BaseTokenAuthenticator<TAccount>
{
	public JwtAuthenticator(IServiceProvider services, string? headerName, string? queryParameterName, string? tokenType) : base(services, headerName, queryParameterName, tokenType)
	{
	}

	protected override Task<TAccount?> Authenticate(string token)
	{
		if (Resolve<JwtService<TAccount>>().TryValidateToken(token, out Guid userId))
		{
			return Resolve<IGuidRepository<TAccount>>()
				.GetById(userId);
		}

		return Task.FromResult<TAccount?>(default);
	}
}