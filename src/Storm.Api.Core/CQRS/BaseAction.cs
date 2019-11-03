using System;
using System.Net;
using System.Threading.Tasks;
using Storm.Api.Core.Exceptions;

namespace Storm.Api.Core.CQRS
{
	public interface IAction<in TParameter, TOutput>
	{
		Task<TOutput> Execute(TParameter parameter);
	}

	public abstract class BaseAction<TParameter, TOutput> : BaseServiceContainer, IAction<TParameter, TOutput>
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
}