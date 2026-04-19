using Microsoft.Extensions.Configuration;

namespace Storm.Api.Databases.Configurations;

public static class DatabaseConfigurationLoader
{
	public static DatabaseConfigurationBuilder LoadDatabaseConfiguration(this IConfiguration configuration)
	{
		/* configuration keys :
			- type: [AzureSqlServer, SqlServer, MySql, SQLite, SQLiteMemory, Postgres]
			- user: username
			- password: password
			- host: host
			- database: database name
			- encrypt: (bool) encrypt sqlserver connection or not
			- integratedSecurity: (bool) sqlserver parameter
			- timeout: (int) timeout value
			- connectionString: (only if you want to provide the full connection string for mysql or sqlserver)
			- debugLogging: (bool) enable debug logging
			- useLogService: (bool) enable log service
			- HighAvailability (SQL Server only, optional):
				- healthCheckIntervalSeconds: (int, default 5)
				- probeConnectTimeoutSeconds: (int, default 2)
				- allowReadsOnAsyncReplicas: (bool, default false)
				- allowReadFallbackToPrimary: (bool, default true)
				- replicas: array of { host, port } OR { connectionString }
		*/

		if (!Enum.TryParse(configuration["type"], out DatabaseType databaseType))
		{
			throw new ArgumentException("Missing or unrecognized database type");
		}

		string? connectionString = configuration.GetValue<string?>("connectionString", null);
		DatabaseConfigurationBuilder configurationBuilder = new DatabaseConfigurationBuilder()
			.UseDebug(configuration.GetValue("debugLogging", false))
			.UseLogService(configuration.GetValue("useLogService", false));


		if (connectionString is null)
		{
			switch (databaseType)
			{
				case DatabaseType.AzureSqlServer:
					configurationBuilder.UseAzureSqlServer(configuration["host"]!,
						configuration["database"]!,
						configuration["user"]!,
						configuration["password"]!,
						configuration.GetValue("encrypt", false),
						configuration.GetValue("timeout", 30));
					break;
				case DatabaseType.SqlServer:
					configurationBuilder.UseSqlServer(configuration["host"]!,
						configuration["database"]!,
						configuration["user"]!,
						configuration["password"]!,
						configuration.GetValue("encrypt", false),
						configuration.GetValue("integratedSecurity", false),
						configuration.GetValue("timeout", 30));
					break;
				case DatabaseType.MySql:
					return configurationBuilder
						.UseMySQL(configuration["host"]!,
							configuration["database"]!,
							configuration["user"]!,
							configuration["password"]!);
				case DatabaseType.Postgres:
					return configurationBuilder
						.UsePostgres(configuration["host"]!,
							configuration["database"]!,
							configuration["user"]!,
							configuration["password"]!);
				case DatabaseType.SQLite:
					return configurationBuilder
						.UseSQLite(configuration["file"]!);
				case DatabaseType.SQLiteMemory:
					return configurationBuilder
						.UseInMemorySQLite();
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		else
		{
			switch (databaseType)
			{
				case DatabaseType.SqlServer:
				case DatabaseType.AzureSqlServer:
					configurationBuilder.UseSqlServer(connectionString);
					break;
				case DatabaseType.MySql:
					return configurationBuilder.UseMySQL(connectionString);
				case DatabaseType.Postgres:
					return configurationBuilder.UsePostgres(connectionString);
				case DatabaseType.SQLite:
				case DatabaseType.SQLiteMemory:
					throw new InvalidOperationException($"Connection string not supported for {nameof(DatabaseType.SQLite)}, {nameof(DatabaseType.SQLiteMemory)}");
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		ApplyHighAvailabilitySection(configurationBuilder, configuration.GetSection("HighAvailability"));
		return configurationBuilder;
	}

	private static void ApplyHighAvailabilitySection(DatabaseConfigurationBuilder builder, IConfigurationSection haSection)
	{
		if (!haSection.Exists())
		{
			return;
		}

		int intervalSeconds = haSection.GetValue("healthCheckIntervalSeconds", 0);
		if (intervalSeconds > 0)
		{
			builder.UseReplicaHealthCheckInterval(TimeSpan.FromSeconds(intervalSeconds));
		}

		int probeTimeoutSeconds = haSection.GetValue("probeConnectTimeoutSeconds", 0);
		if (probeTimeoutSeconds > 0)
		{
			builder.UseReplicaProbeConnectTimeout(TimeSpan.FromSeconds(probeTimeoutSeconds));
		}

		if (haSection.GetValue<bool?>("allowReadsOnAsyncReplicas") is { } allowAsync)
		{
			builder.AllowReadsOnAsyncReplicas(allowAsync);
		}

		if (haSection.GetValue<bool?>("allowReadFallbackToPrimary") is { } allowFallback)
		{
			builder.AllowReadFallbackToPrimary(allowFallback);
		}

		foreach (IConfigurationSection replicaSection in haSection.GetSection("replicas").GetChildren())
		{
			string? replicaCs = replicaSection["connectionString"];
			if (!string.IsNullOrWhiteSpace(replicaCs))
			{
				builder.AddReadReplica(replicaCs);
				continue;
			}

			string? host = replicaSection["host"];
			if (string.IsNullOrWhiteSpace(host))
			{
				throw new InvalidOperationException("Each HighAvailability replica must provide either 'host' or 'connectionString'.");
			}

			int port = replicaSection.GetValue("port", 1433);
			builder.AddReadReplica(host, port);
		}
	}

	private enum DatabaseType
	{
		AzureSqlServer,
		SqlServer,
		MySql,
		SQLite,
		SQLiteMemory,
		Postgres,
	}
}
