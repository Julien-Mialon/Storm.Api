namespace Storm.Api.Core.Services;

public abstract class BaseService
{
	protected IServiceProvider Services { get; }

	protected BaseService(IServiceProvider services)
	{
		Services = services;
	}
}