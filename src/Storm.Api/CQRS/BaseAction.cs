using System.Net;
using Storm.Api.CQRS.Exceptions;
using Storm.Api.Databases.Services;

namespace Storm.Api.CQRS;

public abstract class BaseAction<TParameter, TOutput> : BaseDatabaseService, IAction<TParameter, TOutput>
{
	protected BaseAction(IServiceProvider services) : base(services)
	{
	}

	public virtual Task<TOutput> Execute(TParameter parameter)
	{
		if (!ValidateParameter(parameter))
		{
			throw new DomainHttpCodeException(HttpStatusCode.BadRequest);
		}

		PrepareParameter(parameter);
		return Action(parameter);
	}

	protected virtual bool ValidateParameter(TParameter parameter)
	{
		return parameter != null;
	}

	protected virtual void PrepareParameter(TParameter parameter)
	{
	}

	protected abstract Task<TOutput> Action(TParameter parameter);
}