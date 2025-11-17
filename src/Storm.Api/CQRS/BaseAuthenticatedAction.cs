using System.Net;
using Storm.Api.CQRS.Exceptions;
using Storm.Api.CQRS.Extensions;
using Storm.Api.Databases.Services;

namespace Storm.Api.CQRS;

public abstract class BaseAuthenticatedAction<TParameter, TOutput, TAccount> : BaseDatabaseService, IAction<TParameter, TOutput>
{
	private readonly IActionAuthenticator<TAccount> _authenticator;

	protected BaseAuthenticatedAction(IServiceProvider services) : base(services)
	{
		_authenticator = Resolve<IActionAuthenticator<TAccount>>();
	}

	protected virtual bool ValidateParameter(TParameter parameter)
	{
		return parameter is not null;
	}

	protected virtual void PrepareParameter(TParameter parameter)
	{
	}

	protected virtual Task Authorize(TParameter parameter, TAccount account)
	{
		return Task.CompletedTask;
	}

	public virtual async Task<TOutput> Execute(TParameter parameter)
	{
		if (!ValidateParameter(parameter))
		{
			throw new DomainHttpCodeException(HttpStatusCode.BadRequest);
		}

		PrepareParameter(parameter);
		TAccount account = await _authenticator.Authenticate().UnauthorizedIfNull();

		await Authorize(parameter, account);
		return await Action(parameter, account);
	}

	protected abstract Task<TOutput> Action(TParameter parameter, TAccount account);
}