using System.Data;
using ServiceStack.OrmLite;

namespace Storm.Api.Databases.Migrations.Extensions;

public static class TableExtensions
{
	public static void DropTableIfExists<TModel>(this IDbConnection db)
	{
		if (db.TableExists<TModel>())
		{
			db.DropTable<TModel>();
		}
	}

	public static void DropTableIfExists(this IDbConnection db, string tableName)
	{
		db.ExecuteNonQuery($"DROP TABLE IF EXISTS {tableName}");
	}
}