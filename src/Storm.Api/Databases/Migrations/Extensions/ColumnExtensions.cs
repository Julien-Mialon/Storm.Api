using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using ServiceStack.OrmLite;
using Storm.Api.Databases.Extensions;

namespace Storm.Api.Databases.Migrations.Extensions;

public static class ColumnExtensions
{
	public static bool ColumnExists<TModel>(this IDbConnection db, string columnName)
	{
		return db.ColumnExists(columnName, typeof(TModel).TableRef());
	}

	public static void DropColumnIfExists<TModel>(this IDbConnection db, string columnName)
	{
		if (db.ColumnExists<TModel>(columnName))
		{
			db.DropColumn<TModel>(columnName);
		}
	}

	public static void AddColumn<TModel>(this IDbConnection db, string columnName, Type columnType, bool isNullable, string? defaultValue = null)
	{
		db.AddColumn(typeof(TModel), new()
		{
			Name = columnName,
			FieldType = columnType,
			IsNullable = isNullable,
			DefaultValue = defaultValue,
		});
	}

	public static void AddColumnIfNotExists<TModel>(this IDbConnection db, string columnName)
	{
		if (db.ColumnExists<TModel>(columnName))
		{
			return;
		}

		PropertyInfo? property = typeof(TModel).GetProperty(columnName, BindingFlags.Instance | BindingFlags.Public);
		if (property is null)
		{
			return;
		}

		ParameterExpression parameter = Expression.Parameter(typeof(TModel), "x");
		UnaryExpression body = Expression.Convert(Expression.Property(parameter, property), typeof(object));
		Expression<Func<TModel, object>> expression = Expression.Lambda<Func<TModel, object>>(body, parameter);

		db.AddColumn(expression);
	}

	public static void AddColumnIfNotExists<TModel>(this IDbConnection db, Expression<Func<TModel, object>> field)
	{
		if (db.ColumnExists(field))
		{
			return;
		}

		db.AddColumn(field);
	}
}