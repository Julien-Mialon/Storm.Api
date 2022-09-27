namespace Storm.Api.Authenticators;

public class ConstantApiKeyAuthenticator<TAccount> : BaseConstantApiKeyAuthenticator<TAccount>
{
	public ConstantApiKeyAuthenticator(IServiceProvider services, string apiKey, string headerName, string queryParameterName) : base(services, apiKey, headerName, queryParameterName)
	{
	}

	protected override TAccount? CreateAccount()
	{
		return default;
	}
}