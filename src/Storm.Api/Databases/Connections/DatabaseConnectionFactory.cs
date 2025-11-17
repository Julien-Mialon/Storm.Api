using System.Data;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Storm.Api.Databases.Connections;

internal class DatabaseConnectionFactory : IDatabaseConnectionFactory
{
	private readonly IDbConnectionFactory _factory;

	/// <summary>
	/// Create a new instance of factory with the underlying connection factory
	/// </summary>
	/// <param name="factory">The underlying connection factory</param>
	public DatabaseConnectionFactory(IDbConnectionFactory factory)
	{
		_factory = factory;
	}

	/// <inheritdoc />
	public IDbConnection Create() => _factory.CreateDbConnection();

	/// <inheritdoc />
	public Task<IDbConnection> Open(CancellationToken ct) => _factory.OpenAsync(ct);
}