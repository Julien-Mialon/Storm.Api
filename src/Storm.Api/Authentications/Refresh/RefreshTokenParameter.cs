namespace Storm.Api.Authentications.Refresh;

public class RefreshTokenParameter : IRefreshTokenParameterConvertible
{
	public required string RefreshToken { get; set; }

	public RefreshTokenParameter AsRefreshTokenParameter()
	{
		return this;
	}
}

public interface IRefreshTokenParameterConvertible
{
	RefreshTokenParameter AsRefreshTokenParameter();
}