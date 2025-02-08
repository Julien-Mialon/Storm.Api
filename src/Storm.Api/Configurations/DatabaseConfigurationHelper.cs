using Microsoft.Extensions.Configuration;
using Storm.Api.Core.Databases;

namespace Storm.Api.Configurations;

public static class DatabaseConfigurationHelper
{
	private enum DatabaseType
	{
		AzureSqlServer,
		SqlServer,
		MySql,
		SQLite,
		SQLiteMemory
	}

	public static DatabaseConfigurationBuilder LoadDatabaseConfiguration(this IConfiguration configuration)
	{
		/* configuration keys :
			- type: [mysql, sqlserver, sqlite]
			- user: username
			- password: password
			- host: host
			- database: database name
			- encrypt: (bool) encrypt sqlserver connection or not
			- connectionString: (only if you want to provide the full connection string for mysql or sqlserver)
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
					return configurationBuilder
						.UseAzureSqlServer(
							configuration.GetValue<string>("host")!,
							configuration.GetValue<string>("database")!,
							configuration.GetValue<string>("user")!,
							configuration.GetValue<string>("password")!,
							configuration.GetValue("encrypt", false),
							configuration.GetValue("timeout", 30)
						);
				case DatabaseType.SqlServer:
					return configurationBuilder
						.UseSqlServer(
							configuration.GetValue<string>("host")!,
							configuration.GetValue<string>("database")!,
							configuration.GetValue<string>("user")!,
							configuration.GetValue<string>("password")!,
							configuration.GetValue("encrypt", false),
							configuration.GetValue("integratedSecurity", false),
							configuration.GetValue("timeout", 30)
						);
				case DatabaseType.MySql:
					return configurationBuilder
						.UseMySQL(
							configuration.GetValue<string>("host")!,
							configuration.GetValue<string>("database")!,
							configuration.GetValue<string>("user")!,
							configuration.GetValue<string>("password")!
						);
				case DatabaseType.SQLite:
					return configurationBuilder
						.UseSQLite(configuration.GetValue<string>("file")!);
				case DatabaseType.SQLiteMemory:
					return configurationBuilder
						.UseInMemorySQLite();
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		switch (databaseType)
		{
			case DatabaseType.SqlServer:
				return configurationBuilder.UseSqlServer(connectionString);
			case DatabaseType.MySql:
				return configurationBuilder.UseMySQL(connectionString);
			case DatabaseType.AzureSqlServer:
			case DatabaseType.SQLite:
			case DatabaseType.SQLiteMemory:
				throw new InvalidOperationException($"Connection string not supported for {nameof(DatabaseType.AzureSqlServer)}, {nameof(DatabaseType.SQLite)}, {nameof(DatabaseType.SQLiteMemory)}");
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
}