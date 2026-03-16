namespace Storm.Api.Services;

public interface IScopedServiceAccessor
{
	IServiceProvider Services { get; }

	TService Get<TService>() where TService : notnull;
}