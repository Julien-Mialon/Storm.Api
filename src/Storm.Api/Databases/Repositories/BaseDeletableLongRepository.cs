using ServiceStack.OrmLite;
using Storm.Api.Databases.Extensions;
using Storm.Api.Databases.Models;
using Storm.Api.Databases.Services;

namespace Storm.Api.Databases.Repositories;

internal class BaseDeletableLongRepository<TEntity> : BaseDatabaseService, ILongRepository<TEntity>
	where TEntity : ILongEntity, ISoftDeleteEntity
{
	public BaseDeletableLongRepository(IServiceProvider services) : base(services)
	{
#if DEBUG
		if (typeof(TEntity).IsAssignableTo(typeof(ISoftDeleteEntity)) is false)
		{
			throw new InvalidOperationException($"You should use the BaseLongRepository instead of the BaseDeletableLongRepository for entity {typeof(TEntity)}");
		}
#endif
	}

	public async Task<TEntity?> GetById(long id)
	{
		return await UseReadConnection(async connection =>
		{
			return await connection.From<TEntity>()
				.Where(x => x.Id == id)
				.NotDeleted()
				.AsSingleAsync(connection);
		});
	}

	public async Task<List<TEntity>> List()
	{
		return await UseReadConnection(async connection =>
		{
			return await connection.From<TEntity>()
				.NotDeleted()
				.AsSelectAsync(connection);
		});
	}

	public async Task<TEntity> Create(TEntity entity)
	{
		return await UseWriteConnection(async connection =>
		{
			entity.Id = await connection.InsertAsync(entity, selectIdentity: true);
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

	public async Task<bool> Delete(long id)
	{
		return await UseWriteConnection(async connection =>
		{
			int lines = await connection.UpdateAsync<TEntity>(new
			{
				IsDeleted = true,
				EntityDeletedDate = DateTime.UtcNow
			}, x => x.Id == id);

			return lines > 0;
		});
	}

	public async Task<bool> Delete(TEntity entity)
	{
		return await UseWriteConnection(async connection =>
		{
			entity.IsDeleted = true;
			int lines = await connection.UpdateAsync(entity);
			return lines > 0;
		});
	}
}