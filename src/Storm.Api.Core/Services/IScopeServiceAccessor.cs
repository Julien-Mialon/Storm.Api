namespace Storm.Api.Core.Services;

public interface IScopeServiceAccessor
{
	TService Get<TService>() where TService : notnull;

	IServiceProvider Services { get; }
}