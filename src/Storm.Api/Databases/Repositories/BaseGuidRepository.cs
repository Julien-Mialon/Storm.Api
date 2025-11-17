using Storm.Api.Databases.Models;
using Storm.Api.Databases.Services;

namespace Storm.Api.Databases.Repositories;

public class BaseGuidRepository<TEntity> : BaseDatabaseService, IGuidRepository<TEntity> where TEntity : IGuidEntity
{
	private readonly IGuidRepository<TEntity> _guidRepositoryImplementation;

	public BaseGuidRepository(IServiceProvider services) : base(services)
	{
		_guidRepositoryImplementation = RepositoryShenanigans.CreateGuidRepository<TEntity>(services);
	}

	public Task<TEntity?> GetById(Guid id)
	{
		return _guidRepositoryImplementation.GetById(id);
	}

	public Task<List<TEntity>> List()
	{
		return _guidRepositoryImplementation.List();
	}

	public Task<TEntity> Create(TEntity entity)
	{
		return _guidRepositoryImplementation.Create(entity);
	}

	public Task Create(List<TEntity> entities)
	{
		return _guidRepositoryImplementation.Create(entities);
	}

	public Task<TEntity> Update(TEntity entity)
	{
		return _guidRepositoryImplementation.Update(entity);
	}

	public Task Update(List<TEntity> entity)
	{
		return _guidRepositoryImplementation.Update(entity);
	}

	public Task<bool> Delete(Guid id)
	{
		return _guidRepositoryImplementation.Delete(id);
	}

	public Task<bool> Delete(TEntity entity)
	{
		return _guidRepositoryImplementation.Delete(entity);
	}
}