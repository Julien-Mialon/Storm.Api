using System.Data;
using ServiceStack.Data;

namespace Storm.Api.Databases.Connections.HighAvailability;

internal sealed class HighAvailabilityDatabaseConnectionFactory : IDatabaseConnectionFactory
{
	private readonly IDbConnectionFactory _seedFactory;
	private readonly IDatabaseReplicaManager _manager;

	/// <param name="seedFactory">Used only for <see cref="Create"/> (unopened connection object).</param>
	/// <param name="manager">Resolves the routing for <see cref="OpenWrite"/> / <see cref="OpenRead"/>.</param>
	public HighAvailabilityDatabaseConnectionFactory(IDbConnectionFactory seedFactory, IDatabaseReplicaManager manager)
	{
		_seedFactory = seedFactory;
		_manager = manager;
	}

	public IDbConnection Create()
		=> _seedFactory.CreateDbConnection();

	public Task<IDbConnection> OpenWrite(CancellationToken ct)
		=> _manager.OpenWriteAsync(ct);

	public Task<IDbConnection> OpenRead(CancellationToken ct)
		=> _manager.OpenReadAsync(ct);

	public void EnsurePrimaryAvailable()
		=> _manager.EnsurePrimaryAvailable();

	public bool SupportsReadReplicas => true;
}
