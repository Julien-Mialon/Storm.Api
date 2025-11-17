namespace Storm.Api.Databases.Repositories;

public interface IRepository<TEntity, TKey>
{
	Task<TEntity?> GetById(TKey id);

	Task<List<TEntity>> List();

	Task<TEntity> Create(TEntity entity);

	Task Create(List<TEntity> entities);

	Task<TEntity> Update(TEntity entity);

	Task Update(List<TEntity> entity);

	Task<bool> Delete(TKey id);

	Task<bool> Delete(TEntity entity);
}