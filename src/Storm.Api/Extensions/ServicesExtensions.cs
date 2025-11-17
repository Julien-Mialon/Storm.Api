using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Storm.Api.CQRS;
using Storm.Api.Services;

namespace Storm.Api.Extensions;

public static class ServicesExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DateTime Now(this IServiceProvider provider)
	{
		return provider.GetRequiredService<IDateService>().Now;
	}

	public static T Create<T>(this IServiceProvider provider)
	{
		return ActivatorUtilities.CreateInstance<T>(provider);
	}

	public static async Task ExecuteWithScope(this IServiceProvider services, Func<IServiceProvider, Task> action)
	{
		using IServiceScope scope = services.CreateScope();
		await action(scope.ServiceProvider);
	}

	public static void ExecuteWithScope(this IServiceProvider services, Action<IServiceProvider> action)
	{
		using IServiceScope scope = services.CreateScope();
		action(scope.ServiceProvider);
	}

	public static async Task<TResult> ExecuteWithScope<TResult>(this IServiceProvider services, Func<IServiceProvider, Task<TResult>> action)
	{
		using IServiceScope scope = services.CreateScope();
		return await action(scope.ServiceProvider);
	}

	public static TResult ExecuteWithScope<TResult>(this IServiceProvider services, Func<IServiceProvider, TResult> action)
	{
		using IServiceScope scope = services.CreateScope();
		return action(scope.ServiceProvider);
	}

	public static Task<TOutput> ExecuteAction<TAction, TParameter, TOutput>(this IServiceProvider services, TParameter parameter) where TAction : IAction<TParameter, TOutput>
	{
		TAction action = ActivatorUtilities.CreateInstance<TAction>(services);
		return action.Execute(parameter);
	}
}