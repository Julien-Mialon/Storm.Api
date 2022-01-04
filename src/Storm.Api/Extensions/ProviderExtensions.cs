using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Core.CQRS;

namespace Storm.Api.Extensions;

public static class ProviderExtensions
{
	public static Task<TOutput> ExecuteAction<TAction, TParameter, TOutput>(this IServiceProvider services,TParameter parameter) where TAction : IAction<TParameter, TOutput>
	{
		TAction action = ActivatorUtilities.CreateInstance<TAction>(services);
		return action.Execute(parameter);
	}
}