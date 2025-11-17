using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;
using Storm.Api.Databases.Converters.DapperMappers;

namespace Storm.Api.Databases.Converters;

public static class DatabaseConverters
{
	private static bool _isInitialized;

	public static void Initialize()
	{
		if (_isInitialized)
		{
			return;
		}

		_isInitialized = true;
		IOrmLiteDialectProvider[] sqlServerProviders =
		{
			SqlServerDialect.Provider,
			SqlServer2008Dialect.Provider,
			SqlServer2012Dialect.Provider,
			SqlServer2014Dialect.Provider,
			SqlServer2016Dialect.Provider,
			SqlServer2017Dialect.Provider,
			SqlServer2019Dialect.Provider,
			SqlServer2022Dialect.Provider,
		};

		foreach (IOrmLiteDialectProvider sqlServerProvider in sqlServerProviders)
		{
			SetupOnSqlServer(sqlServerProvider);
		}

		SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
		SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
	}

	private static void SetupOnSqlServer(IOrmLiteDialectProvider sqlServerProvider)
	{
		sqlServerProvider.RegisterConverter<DateOnly>(new SqlServerDateOnlyConverter());
		sqlServerProvider.RegisterConverter<TimeOnly>(new SqlServerTimeOnlyConverter());
	}
}