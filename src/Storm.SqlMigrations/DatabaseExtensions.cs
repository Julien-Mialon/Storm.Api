using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using ServiceStack.OrmLite;
using Storm.Api.Core.Extensions;

namespace Storm.SqlMigrations;

public static class DatabaseExtensions
{
	public static bool ColumnExists<TModel>(this IDbConnection db, string columnName)
	{
		return db.ColumnExists(columnName, typeof(TModel).TableName());
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

	public static bool HasForeignKey(this IDbConnection db, string foreignKeyName)
	{
		return db.Exists<string>($@"
				SELECT con.name
				FROM sys.foreign_keys con
				where con.name = '{foreignKeyName}'");
	}

	public static void CreateForeignKey<TSource, TForeign>(this IDbConnection db, Expression<Func<TSource, object>> sourceField, Expression<Func<TForeign, object>> foreignField)
	{
		string sourceFieldName = ModelDefinition<TSource>.Definition.GetFieldDefinition(sourceField).FieldName;
		string foreignFieldName = ModelDefinition<TForeign>.Definition.GetFieldDefinition(foreignField).FieldName;

		db.AddForeignKey(sourceField, foreignField, OnFkOption.NoAction, OnFkOption.NoAction, $"{typeof(TSource).TableName()}_{typeof(TForeign).TableName()}_{sourceFieldName}_{foreignFieldName}");
	}

	public static void RemoveForeignKey<TSource, TForeign>(this IDbConnection db, Expression<Func<TSource, object>> sourceField, Expression<Func<TForeign, object>> foreignField)
	{
		string sourceFieldName = ModelDefinition<TSource>.Definition.GetFieldDefinition(sourceField).FieldName;
		string foreignFieldName = ModelDefinition<TForeign>.Definition.GetFieldDefinition(foreignField).FieldName;

		db.DropForeignKey<TSource>($"{typeof(TSource).TableName()}_{typeof(TForeign).TableName()}_{sourceFieldName}_{foreignFieldName}");
	}

	public static async Task DropSqlServerDefaultConstraint<T>(this IDbConnection db, string columnName)
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