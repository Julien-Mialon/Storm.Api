using ServiceStack.OrmLite;
using Storm.Api.Databases.Extensions;
using Storm.Api.Databases.Models;
using Storm.Api.Databases.Services;

namespace Storm.Api.Databases.Repositories;

internal class BaseNonDeletableGuidRepository<TEntity> : BaseDatabaseService, IGuidRepository<TEntity>
	where TEntity : IGuidEntity
{
	public BaseNonDeletableGuidRepository(IServiceProvider services) : base(services)
	{
#if DEBUG
		if (typeof(TEntity).IsAssignableTo(typeof(ISoftDeleteEntity)))
		{
			throw new InvalidOperationException($"You should use the BaseDeletableGuidRepository instead of the BaseGuidRepository for entity {typeof(TEntity)}");
		}
#endif
	}

	public async Task<TEntity?> GetById(Guid id)
	{
		return await UseReadConnection(async connection =>
		{
			return await connection.From<TEntity>()
				.Where(x => x.Id == id)
				.AsSingleAsync(connection);
		});
	}

	public async Task<List<TEntity>> List()
	{
		return await UseReadConnection(async connection =>
		{
			return await connection.From<TEntity>()
				.AsSelectAsync(connection);
		});
	}

	public async Task<TEntity> Create(TEntity entity)
	{
		return await UseWriteConnection(async connection =>
		{
			await connection.InsertAsync(entity);
			return entity;
		});
	}

	public async Task Create(List<TEntity> entities)
	{
		await UseWriteConnection(async connection =>
		{
			await connection.InsertAllAsync(entities);
		});
	}

	public async Task<TEntity> Update(TEntity entity)
	{
		return await UseWriteConnection(async connection =>
		{
			await connection.UpdateAsync(entity);
			return entity;
		});
	}

	public async Task Update(List<TEntity> entity)
	{
		await UseWriteConnection(async connection =>
		{
			await connection.UpdateAllAsync(entity);
		});
	}

	public async Task<bool> Delete(Guid id)
	{
		return await UseWriteConnection(async connection =>
		{
			int lines = await connection.DeleteByIdAsync<TEntity>(id);
			return lines > 0;
		});
	}

	public async Task<bool> Delete(TEntity entity)
	{
		return await UseWriteConnection(async connection =>
		{
			int lines = await connection.DeleteAsync(entity);
			return lines > 0;
		});
	}
}