using System;
using ServiceStack.Logging;
using ServiceStack.MiniProfiler.Data;
using ServiceStack.OrmLite;
using Storm.Api.Core.Databases.Internals;

namespace Storm.Api.Core.Databases
{
	/// <summary>
	/// Builder to configure and create connection factory.
	/// </summary>
	public class DatabaseConfigurationBuilder
	{
		private const string AZURE_SQL_SERVER_FORMAT = "Server=tcp:{0},1433;Initial Catalog={1};Persist Security Info=False;User ID={2};Password={3};MultipleActiveResultSets=True;Encrypt={4};TrustServerCertificate=False;Connection Timeout=30;";

		private const string SQL_SERVER_FORMAT = "Data Source={0};Initial Catalog={1};Integrated Security=True;User ID={2};Password={3};MultipleActiveResultSets=True;Encrypt={4};TrustServerCertificate=False;Connect Timeout=30;";

		private const string MYSQL_FORMAT = "Server={0};Port=3306;Database={1};UID={2};Password={3};SslMode=None;Charset=utf8";

		private bool _enableDebug;
		private bool _useLogService;
		private string _connectionString;
		private IOrmLiteDialectProvider _dialectProvider;

		/// <summary>
		/// Create database factory from configuration
		/// </summary>
		/// <returns>A new connection factory</returns>
		public IDatabaseConnectionFactory Build()
		{
			if (_useLogService)
			{
				LogManager.LogFactory = new LogServiceLogFactory(debugEnabled: _enableDebug);
			}
			else
			{
				LogManager.LogFactory = new ConsoleLogFactory(debugEnabled: _enableDebug);
			}

			string connectionString = _connectionString ?? throw new InvalidOperationException("Configuration has not been finished");
			IOrmLiteDialectProvider provider = _dialectProvider ?? throw new InvalidOperationException("Configuration has not been finished");
			OrmLiteConnectionFactory connectionFactory = new OrmLiteConnectionFactory(connectionString, provider);

			if (_enableDebug)
			{
				connectionFactory.ConnectionFilter = x => new ProfiledConnection(x, new DebugSqlProfiler());
			}

			IDatabaseConnectionFactory factory = new DatabaseConnectionFactory(connectionFactory);
			OrmLiteConfig.DialectProvider.GetStringConverter().UseUnicode = true;
			OrmLiteInterceptors.Initialize();
			SqlFieldsOrdering.Enable();
			return factory;
		}

		/// <summary>
		/// Configure to use SQLite with specific database file
		/// </summary>
		/// <param name="file">Path to database file</param>
		/// <returns>this</returns>
		public DatabaseConfigurationBuilder UseSQLite(string file)
		{
			_connectionString = file;
			_dialectProvider = SqliteDialect.Provider;
			return this;
		}

		/// <summary>
		/// Configure to use SQLite with in memory configuration (useful for testing)
		/// </summary>
		/// <returns>this</returns>
		public DatabaseConfigurationBuilder UseInMemorySQLite() => UseSQLite(":memory:");

		/// <summary>
		/// Configure to use MySQL with specific connection string
		/// </summary>
		/// <param name="connectionString">Connection string to access MySQL Server</param>
		/// <returns>this</returns>
		public DatabaseConfigurationBuilder UseMySQL(string connectionString)
		{
			_connectionString = connectionString;
			_dialectProvider = MySqlDialect.Provider;
			return this;
		}

		/// <summary>
		/// Configure to use MySQL
		/// </summary>
		/// <param name="host">Host of the MySQL instance</param>
		/// <param name="database">Database to connect</param>
		/// <param name="login">Database user login</param>
		/// <param name="password">Database user password</param>
		/// <returns>this</returns>
		public DatabaseConfigurationBuilder UseMySQL(string host, string database, string login, string password)
			=> UseMySQL(string.Format(MYSQL_FORMAT, host, database, login, password));

		/// <summary>
		/// Configure to use SQL Server with specific connection string
		/// </summary>
		/// <param name="connectionString">Connection string to access SQL Server</param>
		/// <returns>this</returns>
		public DatabaseConfigurationBuilder UseSqlServer(string connectionString)
		{
			_connectionString = connectionString;
			_dialectProvider = SqlServer2012Dialect.Provider;
			return this;
		}

		/// <summary>
		/// Configure to use SQL Server in Azure
		/// </summary>
		/// <param name="host">Host of the SQL Server instance</param>
		/// <param name="database">Database to connect</param>
		/// <param name="login">Database user login</param>
		/// <param name="password">Database user password</param>
		/// <param name="encrypt">Encrypt=True or Encrypt=False in connection string</param>
		/// <returns>this</returns>
		public DatabaseConfigurationBuilder UseSqlServer(string host, string database, string login, string password, bool encrypt = true)
			=> UseSqlServer(string.Format(SQL_SERVER_FORMAT, host, database, login, password, encrypt ? "True" : "False"));

		/// <summary>
		/// Configure to use SQL Server in Azure
		/// </summary>
		/// <param name="host">Host of the SQL Server instance</param>
		/// <param name="database">Database to connect</param>
		/// <param name="login">Database user login</param>
		/// <param name="password">Database user password</param>
		/// <param name="encrypt">Encrypt=True or Encrypt=False in connection string</param>
		/// <returns>this</returns>
		public DatabaseConfigurationBuilder UseAzureSqlServer(string host, string database, string login, string password, bool encrypt = true)
			=> UseSqlServer(string.Format(AZURE_SQL_SERVER_FORMAT, host, database, login, password, encrypt ? "True" : "False"));

		public DatabaseConfigurationBuilder UseDebug(bool enableDebug)
		{
			_enableDebug = enableDebug;
			return this;
		}

		public DatabaseConfigurationBuilder UseLogService(bool useLogService)
		{
			_useLogService = useLogService;
			return this;
		}
	}
}