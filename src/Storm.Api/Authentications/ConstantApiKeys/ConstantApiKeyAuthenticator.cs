namespace Storm.Api.Authentications.ConstantApiKeys;

public class ConstantApiKeyAuthenticator<TAccount> : BaseConstantApiKeyAuthenticator<TAccount>
	where TAccount : new()
{
	public ConstantApiKeyAuthenticator(IServiceProvider services, string apiKey, string headerName, string queryParameterName) : base(services, apiKey, headerName, queryParameterName)
	{
	}

	protected override TAccount? CreateAccount()
	{
		return new TAccount();
	}
}