using System.Data;
using ServiceStack.OrmLite;
using Storm.Api.Databases.Extensions;

namespace Storm.Api.Databases.Migrations.Extensions;

public static class SqlServerForeignKeyExtensions
{
	public static bool SqlServerHasForeignKey(this IDbConnection db, string foreignKeyName)
	{
		return db.Exists<string>($@"
				SELECT con.name
				FROM sys.foreign_keys con
				where con.name = '{foreignKeyName}'");
	}

	public static async Task SqlServerDropDefaultConstraint<T>(this IDbConnection db, string columnName)
	{
		List<string> result = await db.ColumnAsync<string>($@"
				SELECT con.name
				FROM sys.default_constraints con
				LEFT OUTER JOIN sys.objects t ON con.parent_object_id = t.object_id
				LEFT OUTER JOIN sys.all_columns col ON con.parent_column_id = col.column_id AND con.parent_object_id = col.object_id
				WHERE t.name = '{typeof(T).TableName()}' AND col.name = '{columnName}'");

		if (result is null or { Count: 0 })
		{
			return;
		}

		string constraintName = result[0];

		db.AlterTable<T>($"DROP CONSTRAINT {constraintName}");
	}
}