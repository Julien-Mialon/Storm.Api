using Microsoft.Extensions.DependencyInjection;
using Storm.Api.Databases.Models;

namespace Storm.Api.Databases.Repositories;

public static class RepositoriesDependencyInjection
{
	public static IServiceCollection AddRepository<TEntity, TRepository>(this IServiceCollection services)
		where TEntity : IGuidEntity
		where TRepository : class, IGuidRepository<TEntity>
	{
		return services.AddScoped<TRepository>()
			.AddScoped<IGuidRepository<TEntity>>(s => s.GetRequiredService<TRepository>());
	}

	public static IServiceCollection AddLongRepository<TEntity, TRepository>(this IServiceCollection services)
		where TEntity : ILongEntity
		where TRepository : class, ILongRepository<TEntity>
	{
		return services.AddScoped<TRepository>()
			.AddScoped<ILongRepository<TEntity>>(s => s.GetRequiredService<TRepository>());
	}
}