namespace Storm.Api.CQRS;

public interface IActionAuthenticator<TAccount>
{
	Task<TAccount?> Authenticate();
}