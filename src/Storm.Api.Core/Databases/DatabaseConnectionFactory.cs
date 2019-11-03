using System.Data;
using System.Threading.Tasks;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Storm.Api.Core.Databases
{
	/// <summary>
	/// Connection factory to use database
	/// </summary>
	public interface IDatabaseConnectionFactory
	{
		/// <summary>
		/// Create a database connection but do not open it yet
		/// </summary>
		/// <returns>The newly created connection</returns>
		IDbConnection Create();

		/// <summary>
		/// Create a database connection and open it
		/// </summary>
		/// <returns>The opened connection</returns>
		Task<IDbConnection> Open();
	}

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
		public Task<IDbConnection> Open() => _factory.OpenAsync();
	}
}