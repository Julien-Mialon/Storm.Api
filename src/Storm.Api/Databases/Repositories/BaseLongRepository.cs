using Storm.Api.Databases.Models;
using Storm.Api.Databases.Services;

namespace Storm.Api.Databases.Repositories;

public class BaseLongRepository<TEntity> : BaseDatabaseService, ILongRepository<TEntity> where TEntity : ILongEntity
{
	private readonly ILongRepository<TEntity> _longRepositoryImplementation;

	public BaseLongRepository(IServiceProvider services) : base(services)
	{
		_longRepositoryImplementation = RepositoryShenanigans.CreateLongRepository<TEntity>(services);
	}

	public Task<TEntity?> GetById(long id)
	{
		return _longRepositoryImplementation.GetById(id);
	}

	public Task<List<TEntity>> List()
	{
		return _longRepositoryImplementation.List();
	}

	public Task<TEntity> Create(TEntity entity)
	{
		return _longRepositoryImplementation.Create(entity);
	}

	public Task Create(List<TEntity> entities)
	{
		return _longRepositoryImplementation.Create(entities);
	}

	public Task<TEntity> Update(TEntity entity)
	{
		return _longRepositoryImplementation.Update(entity);
	}

	public Task Update(List<TEntity> entity)
	{
		return _longRepositoryImplementation.Update(entity);
	}

	public Task<bool> Delete(long id)
	{
		return _longRepositoryImplementation.Delete(id);
	}

	public Task<bool> Delete(TEntity entity)
	{
		return _longRepositoryImplementation.Delete(entity);
	}
}