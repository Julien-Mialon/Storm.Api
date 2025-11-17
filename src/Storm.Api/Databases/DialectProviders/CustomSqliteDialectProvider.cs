using System.Linq.Expressions;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Sqlite;

namespace Storm.Api.Databases.DialectProviders;

public class CustomSqliteDialectProvider : SqliteOrmLiteDialectProvider
{
	public override string ToAddForeignKeyStatement<T, TForeign>(Expression<Func<T, object>> field, Expression<Func<TForeign, object>> foreignField, OnFkOption onUpdate, OnFkOption onDelete, string? foreignKeyName = null)
	{
		// not supported by sqlite
		return SelectIdentitySql;
	}
}