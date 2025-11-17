namespace Storm.Api.Authentications.Jwts;

public class JwtService<TAccount>
{
	private readonly JwtConfiguration<TAccount> _configuration;
	private readonly IJwtTokenService _tokenService;

	public JwtService(IJwtTokenService tokenService, JwtConfiguration<TAccount> configuration)
	{
		_tokenService = tokenService;
		_configuration = configuration;
	}

	public (string token, TimeSpan duration) GenerateToken(Guid accountId)
	{
		string token = _tokenService.GenerateToken(accountId, _configuration.Audience, _configuration.Issuer, _configuration.Duration, _configuration.Key);
		return (token, _configuration.Duration);
	}

	public bool TryValidateToken(string token, out Guid accountId)
	{
		return _tokenService.TryGetId(token, _configuration.Audience, _configuration.Issuer, _configuration.Key, out accountId);
	}
}