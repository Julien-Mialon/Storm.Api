using System.Data;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;
using Storm.Api.Databases.Extensions;

namespace Storm.Api.Databases.Migrations.Extensions;

public static class SqlServerIndexExtensions
{
	public static async Task SqlServerDropIndexOnColumn<TTable>(this IDbConnection connection, string columnName)
	{
		IEnumerable<dynamic> result = await connection.QueryAsync(@"
			SELECT indexes.Name as Name
				FROM sys.indexes indexes
		        LEFT JOIN sys.tables tables ON indexes.object_id = tables.object_id
		        LEFT JOIN sys.index_columns indexColumns ON indexes.index_id = indexColumns.index_id AND indexes.object_id = indexColumns.object_id
		        LEFT JOIN sys.columns columns ON indexColumns.column_id = columns.column_id AND columns.object_id = indexes.object_id
		    WHERE tables.name = @tableName AND columns.name = @columnName", new
		{
			tableName = typeof(TTable).TableName(),
			columnName = columnName
		});

		foreach (dynamic index in result)
		{
			string indexName = index.Name;
			await connection.ExecuteNonQueryAsync($"DROP INDEX IF EXISTS {indexName} ON {typeof(TTable).TableName()}");
		}
	}
}