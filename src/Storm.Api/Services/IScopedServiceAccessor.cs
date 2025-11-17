namespace Storm.Api.Services;

public interface IScopedServiceAccessor
{
	TService Get<TService>() where TService : notnull;

	IServiceProvider Services { get; }
}