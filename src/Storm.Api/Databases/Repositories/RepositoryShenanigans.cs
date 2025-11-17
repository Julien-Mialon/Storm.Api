using System.Collections.Concurrent;
using System.Linq.Expressions;
using Storm.Api.Databases.Models;

namespace Storm.Api.Databases.Repositories;

internal static class RepositoryShenanigans
{
	private static readonly ConcurrentDictionary<Type, Delegate> GUID_FACTORY_CACHE = new();
	private static readonly ConcurrentDictionary<Type, Delegate> LONG_FACTORY_CACHE = new();

	public static IGuidRepository<TEntity> CreateGuidRepository<TEntity>(IServiceProvider services) where TEntity : IGuidEntity
	{
		if (GUID_FACTORY_CACHE.TryGetValue(typeof(TEntity), out Delegate? factory))
		{
			return (factory as Func<IServiceProvider, IGuidRepository<TEntity>>)!.Invoke(services);
		}

		ParameterExpression servicesParameter = Expression.Parameter(typeof(IServiceProvider), nameof(services));
		ParameterExpression repositoryVariable = Expression.Variable(typeof(IGuidRepository<TEntity>), "repository");

		Type repositoryInstanceType = typeof(TEntity).IsAssignableTo(typeof(ISoftDeleteEntity)) ?
			typeof(BaseDeletableGuidRepository<>).MakeGenericType(typeof(TEntity)) :
			typeof(BaseNonDeletableGuidRepository<TEntity>);

		BlockExpression body = Expression.Block([repositoryVariable],
			Expression.Assign(repositoryVariable, Expression.New(repositoryInstanceType.GetConstructors()[0], servicesParameter)),
			repositoryVariable);
		LambdaExpression lambda = Expression.Lambda(body, servicesParameter);
		Delegate lambdaCall = lambda.Compile();

		GUID_FACTORY_CACHE.TryAdd(typeof(TEntity), lambdaCall);

		return (lambdaCall as Func<IServiceProvider, IGuidRepository<TEntity>>)!.Invoke(services);
	}

	public static ILongRepository<TEntity> CreateLongRepository<TEntity>(IServiceProvider services) where TEntity : ILongEntity
	{
		if (LONG_FACTORY_CACHE.TryGetValue(typeof(TEntity), out Delegate? factory))
		{
			return (factory as Func<IServiceProvider, ILongRepository<TEntity>>)!.Invoke(services);
		}

		ParameterExpression servicesParameter = Expression.Parameter(typeof(IServiceProvider), nameof(services));
		ParameterExpression repositoryVariable = Expression.Variable(typeof(ILongRepository<TEntity>), "repository");

		Type repositoryInstanceType = typeof(TEntity).IsAssignableTo(typeof(ISoftDeleteEntity)) ?
			typeof(BaseDeletableLongRepository<>).MakeGenericType(typeof(TEntity)) :
			typeof(BaseNonDeletableLongRepository<TEntity>);

		BlockExpression body = Expression.Block([repositoryVariable],
			Expression.Assign(repositoryVariable, Expression.New(repositoryInstanceType.GetConstructors()[0], servicesParameter)),
			repositoryVariable);
		LambdaExpression lambda = Expression.Lambda(body, servicesParameter);
		Delegate lambdaCall = lambda.Compile();

		LONG_FACTORY_CACHE.TryAdd(typeof(TEntity), lambdaCall);

		return (lambdaCall as Func<IServiceProvider, ILongRepository<TEntity>>)!.Invoke(services);
	}
}