using Microsoft.Data.SqlClient;
using ServiceStack.Logging;
using ServiceStack.MiniProfiler.Data;
using ServiceStack.OrmLite;
using Storm.Api.Databases.Configurations.HighAvailability;
using Storm.Api.Databases.Connections;
using Storm.Api.Databases.Connections.HighAvailability;
using Storm.Api.Databases.Converters;
using Storm.Api.Databases.DialectProviders;
using Storm.Api.Databases.Internals;

namespace Storm.Api.Databases.Configurations;

/// <summary>
/// Builder to configure and create connection factory.
/// </summary>
public class DatabaseConfigurationBuilder
{
	private const string AZURE_SQL_SERVER_FORMAT = "Server=tcp:{0},1433;Initial Catalog={1};Persist Security Info=False;User ID={2};Password={3};MultipleActiveResultSets=True;Encrypt={4};TrustServerCertificate=False;Connection Timeout={5};";
	private const string SQL_SERVER_FORMAT = "Data Source={0};Initial Catalog={1};Integrated Security={5};User ID={2};Password={3};MultipleActiveResultSets=True;Encrypt={4};TrustServerCertificate=False;Connect Timeout={6};";
	private const string MYSQL_FORMAT = "Server={0};Port={4};Database={1};UID={2};Password={3};SslMode=None;Charset=utf8";
	private const string POSTGRES_FORMAT = "Server={0};Port={1};Userid={2};Password={3};Database={4};Protocol=3;SSL=false;Pooling=false;MinPoolSize=1;MaxPoolSize=20;Timeout=15;SslMode=Disable";

	private bool _enableDebug;
	private bool _useLogService;
	private bool _isSqlServer;
	private string? _connectionString;
	private IOrmLiteDialectProvider? _dialectProvider;

	private DatabaseInterceptorDelegate? _onInsert;
	private DatabaseInterceptorDelegate? _onUpdate;
	private TimeProvider _timeProvider = TimeProvider.System;

	private readonly List<ReplicaDefinition> _replicas = new();

	internal IReadOnlyList<ReplicaDefinition> ReplicaDefinitions => _replicas;
	internal HighAvailabilityOptions? HighAvailabilityOptionsOrNull { get; private set; }
	internal IDatabaseReplicaManager? BuiltManager { get; private set; }

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
		OrmLiteConnectionFactory connectionFactory = new(connectionString, provider);

		if (_enableDebug)
		{
			connectionFactory.ConnectionFilter = x => new ProfiledConnection(x, new DebugSqlProfiler());
		}

		IDatabaseConnectionFactory factory;
		if (_replicas.Count > 0)
		{
			if (!_isSqlServer)
			{
				throw new InvalidOperationException("Read replicas are only supported on SQL Server. Call UseSqlServer(...) first.");
			}

			HighAvailabilityOptions haOptions = HighAvailabilityOptionsOrNull ??= new HighAvailabilityOptions();
			(string primaryHost, int primaryPort) = ParseSqlServerDataSource(connectionString);
			List<ReplicaDefinition> all = new(_replicas.Count + 1)
			{
				new ReplicaDefinition(primaryHost, primaryPort, connectionString),
			};
			all.AddRange(_replicas);

			DatabaseReplicaManager manager = new(all, provider, haOptions, logService: null);
			BuiltManager = manager;
			factory = new HighAvailabilityDatabaseConnectionFactory(connectionFactory, manager);
		}
		else
		{
			factory = new DatabaseConnectionFactory(connectionFactory);
		}

		OrmLiteConfig.DialectProvider.GetStringConverter().UseUnicode = true;

		DatabaseConverters.Initialize();
		OrmLiteInterceptors.Initialize(_onInsert, _onUpdate, _timeProvider);
		SequentialGuid.Initialize(_isSqlServer, _timeProvider);
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
		_dialectProvider = new CustomSqliteDialectProvider();
		return this;
	}

	/// <summary>
	/// Configure to use SQLite with in memory configuration (useful for testing)
	/// </summary>
	/// <returns>this</returns>
	public DatabaseConfigurationBuilder UseInMemorySQLite()
		=> UseSQLite(":memory:");

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
	{
		int port = 3306;
		if (host.Contains(":"))
		{
			string[] split = host.Split(':');
			host = split[0];
			port = int.Parse(split[1]);
		}

		return UseMySQL(string.Format(MYSQL_FORMAT, host, database, login, password, port));
	}

	/// <summary>
	/// Configure to use SQL Server with specific connection string
	/// </summary>
	/// <param name="connectionString">Connection string to access SQL Server</param>
	/// <returns>this</returns>
	public DatabaseConfigurationBuilder UseSqlServer(string connectionString)
	{
		_connectionString = connectionString;
		_dialectProvider = SqlServer2022Dialect.Provider;
		_isSqlServer = true;
		return this;
	}

	/// <summary>
	/// Configure to use SQL Server
	/// </summary>
	/// <param name="host">Host of the SQL Server instance</param>
	/// <param name="database">Database to connect</param>
	/// <param name="login">Database user login</param>
	/// <param name="password">Database user password</param>
	/// <param name="encrypt">Encrypt=True or Encrypt=False in connection string</param>
	/// <param name="integratedSecurity">IntegratedSecurity=True/False in the connection string, should be disabled unless you are using an active directory with same domain</param>
	/// <param name="timeout">timeout of the connection</param>
	/// <returns>this</returns>
	public DatabaseConfigurationBuilder UseSqlServer(string host, string database, string login, string password, bool encrypt = true, bool integratedSecurity = false, int timeout = 30)
		=> UseSqlServer(string.Format(SQL_SERVER_FORMAT, host, database, login, password, encrypt ? "True" : "False", integratedSecurity ? "True" : "False", timeout));

	/// <summary>
	/// Configure to use SQL Server in Azure
	/// </summary>
	/// <param name="host">Host of the SQL Server instance</param>
	/// <param name="database">Database to connect</param>
	/// <param name="login">Database user login</param>
	/// <param name="password">Database user password</param>
	/// <param name="encrypt">Encrypt=True or Encrypt=False in connection string</param>
	/// <param name="timeout">timeout of the connection</param>
	/// <returns>this</returns>
	public DatabaseConfigurationBuilder UseAzureSqlServer(string host, string database, string login, string password, bool encrypt = true, int timeout = 30)
		=> UseSqlServer(string.Format(AZURE_SQL_SERVER_FORMAT, host, database, login, password, encrypt ? "True" : "False", timeout));

	public DatabaseConfigurationBuilder UsePostgres(string connectionString)
	{
		_connectionString = connectionString;
		_dialectProvider = PostgreSqlDialect.Provider;
		return this;
	}

	public DatabaseConfigurationBuilder UsePostgres(string host, string database, string login, string password)
	{
		int port = 5432;
		if (host.Contains(":"))
		{
			string[] split = host.Split(':');
			host = split[0];
			port = int.Parse(split[1]);
		}

		return UsePostgres(string.Format(POSTGRES_FORMAT, host, port, login, password, database));
	}


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

	public DatabaseConfigurationBuilder UseInterceptorOnInsert(DatabaseInterceptorDelegate onInsert)
	{
		_onInsert = onInsert;
		return this;
	}

	public DatabaseConfigurationBuilder UseInterceptorOnUpdate(DatabaseInterceptorDelegate onUpdate)
	{
		_onUpdate = onUpdate;
		return this;
	}

	public DatabaseConfigurationBuilder UseTimeProvider(TimeProvider timeProvider)
	{
		_timeProvider = timeProvider;
		return this;
	}

	// ------------------------------------------------------------------
	// SQL Server High Availability configuration
	// ------------------------------------------------------------------

	/// <summary>
	/// Tweak advanced high-availability options (health check interval, probe timeout, callbacks).
	/// Requires <see cref="UseSqlServer(string)"/> or one of its overloads first.
	/// </summary>
	public DatabaseConfigurationBuilder UseSqlServerHighAvailability(Action<HighAvailabilityOptions> configure)
	{
		EnsureSqlServer();
		HighAvailabilityOptionsOrNull ??= new HighAvailabilityOptions();
		configure(HighAvailabilityOptionsOrNull);
		return this;
	}

	/// <summary>
	/// Register a read replica reusing the credentials and database of the primary connection string,
	/// swapping only the data source host/port.
	/// </summary>
	public DatabaseConfigurationBuilder AddReadReplica(string host, int port = 1433)
	{
		EnsureSqlServer();
		string primaryConnectionString = _connectionString ?? throw new InvalidOperationException("AddReadReplica(host, port) must be called after UseSqlServer(...).");
		SqlConnectionStringBuilder builder = new(primaryConnectionString)
		{
			DataSource = port is 1433 or 0 ? host : $"{host},{port.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
		};
		_replicas.Add(new ReplicaDefinition(host, port == 0 ? 1433 : port, builder.ConnectionString));
		return this;
	}

	/// <summary>
	/// Register a read replica with a full connection string (useful when replicas use different credentials).
	/// </summary>
	public DatabaseConfigurationBuilder AddReadReplica(string connectionString)
	{
		EnsureSqlServer();
		(string host, int port) = ParseSqlServerDataSource(connectionString);
		_replicas.Add(new ReplicaDefinition(host, port, connectionString));
		return this;
	}

	/// <summary>
	/// Allow routing reads to secondaries even if they're not in SYNCHRONOUS_COMMIT / SYNCHRONIZED mode.
	/// Default: false (only synchronous replicas are eligible).
	/// </summary>
	public DatabaseConfigurationBuilder AllowReadsOnAsyncReplicas(bool value = true)
	{
		HighAvailabilityOptionsOrNull ??= new HighAvailabilityOptions();
		HighAvailabilityOptionsOrNull.AllowReadsOnAsyncReplicas = value;
		return this;
	}

	/// <summary>
	/// Control whether <c>UseReadConnection</c> falls back to the primary when no eligible secondary
	/// is available. Default: true.
	/// </summary>
	public DatabaseConfigurationBuilder AllowReadFallbackToPrimary(bool value = true)
	{
		HighAvailabilityOptionsOrNull ??= new HighAvailabilityOptions();
		HighAvailabilityOptionsOrNull.AllowReadFallbackToPrimary = value;
		return this;
	}

	/// <summary>Set how frequently the health worker probes every configured replica.</summary>
	public DatabaseConfigurationBuilder UseReplicaHealthCheckInterval(TimeSpan interval)
	{
		HighAvailabilityOptionsOrNull ??= new HighAvailabilityOptions();
		HighAvailabilityOptionsOrNull.HealthCheckInterval = interval;
		return this;
	}

	/// <summary>Set the connect timeout applied to each health probe attempt.</summary>
	public DatabaseConfigurationBuilder UseReplicaProbeConnectTimeout(TimeSpan timeout)
	{
		HighAvailabilityOptionsOrNull ??= new HighAvailabilityOptions();
		HighAvailabilityOptionsOrNull.ProbeConnectTimeout = timeout;
		return this;
	}

	public DatabaseConfigurationBuilder OnPrimarySwitched(OnPrimarySwitched callback)
	{
		HighAvailabilityOptionsOrNull ??= new HighAvailabilityOptions();
		HighAvailabilityOptionsOrNull.PrimarySwitched = callback;
		return this;
	}

	public DatabaseConfigurationBuilder OnPrimaryUnavailable(OnPrimaryUnavailable callback)
	{
		HighAvailabilityOptionsOrNull ??= new HighAvailabilityOptions();
		HighAvailabilityOptionsOrNull.PrimaryUnavailable = callback;
		return this;
	}

	public DatabaseConfigurationBuilder OnPrimaryRestored(OnPrimaryRestored callback)
	{
		HighAvailabilityOptionsOrNull ??= new HighAvailabilityOptions();
		HighAvailabilityOptionsOrNull.PrimaryRestored = callback;
		return this;
	}

	public DatabaseConfigurationBuilder OnReplicaStateChanged(OnReplicaStateChanged callback)
	{
		HighAvailabilityOptionsOrNull ??= new HighAvailabilityOptions();
		HighAvailabilityOptionsOrNull.ReplicaStateChanged = callback;
		return this;
	}

	private void EnsureSqlServer()
	{
		if (!_isSqlServer)
		{
			throw new InvalidOperationException("High availability features are only supported for SQL Server. Call UseSqlServer(...) first.");
		}
	}

	internal static (string Host, int Port) ParseSqlServerDataSource(string connectionString)
	{
		SqlConnectionStringBuilder builder = new(connectionString);
		string dataSource = builder.DataSource ?? string.Empty;
		if (dataSource.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase))
		{
			dataSource = dataSource[4..];
		}

		string hostPart = dataSource;
		int port = 1433;
		int commaIdx = dataSource.IndexOf(',');
		if (commaIdx >= 0)
		{
			hostPart = dataSource[..commaIdx];
			if (!int.TryParse(dataSource[(commaIdx + 1)..], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out port) || port == 0)
			{
				port = 1433;
			}
		}

		int backslashIdx = hostPart.IndexOf('\\');
		if (backslashIdx >= 0)
		{
			hostPart = hostPart[..backslashIdx];
		}

		return (hostPart, port);
	}
}
