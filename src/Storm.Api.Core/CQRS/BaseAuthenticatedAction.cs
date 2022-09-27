using System.Net;
using Storm.Api.Core.Exceptions;

namespace Storm.Api.Core.CQRS;

public interface IActionAuthenticator<TAccount, in TAuthenticationParameter>
{
	Task<(bool authenticated, TAccount? account)> Authenticate(TAuthenticationParameter parameter);
}

public abstract class BaseAuthenticatedAction<TParameter, TOutput, TAccount, TAuthenticatorParameter> : BaseAction<TParameter, TOutput>
{
	private readonly TAuthenticatorParameter _authenticatorParameter;
	private readonly IActionAuthenticator<TAccount, TAuthenticatorParameter> _authenticator;

	protected TAccount? Account { get; private set; }

	public BaseAuthenticatedAction(IServiceProvider services, TAuthenticatorParameter authenticatorParameter) : base(services)
	{
		_authenticatorParameter = authenticatorParameter;
		_authenticator = Resolve<IActionAuthenticator<TAccount, TAuthenticatorParameter>>();
	}

	public override async Task<TOutput> Execute(TParameter parameter)
	{
		(bool authenticated, TAccount? account) = await _authenticator.Authenticate(_authenticatorParameter);

		if (!authenticated)
		{
			throw new DomainHttpCodeException(HttpStatusCode.Unauthorized);
		}

		await Authorize(parameter, account);

		Account = account;

		return await base.Execute(parameter);
	}

	protected virtual Task Authorize(TParameter parameter, TAccount? account)
	{
		return Task.CompletedTask;
	}
}