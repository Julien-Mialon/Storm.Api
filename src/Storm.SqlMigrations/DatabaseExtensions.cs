using System;
using System.Data;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Storm.Api.Core.Extensions;

namespace Storm.SqlMigrations
{
	public static class DatabaseExtensions
	{
		public static bool ColumnExists<TModel>(this IDbConnection db, string columnName)
		{
			return db.ColumnExists(columnName, typeof(TModel).TableName());
		}

		public static void AddColumn<TModel>(this IDbConnection db, string columnName, Type columnType, bool isNullable, string defaultValue = null)
		{
			db.AddColumn(typeof(TModel), new FieldDefinition
			{
				Name = columnName,
				FieldType = columnType,
				IsNullable = isNullable,
				DefaultValue = defaultValue,
			});
		}

		public static void CreateForeignKey<TSource, TForeign>(this IDbConnection db, Expression<Func<TSource, object>> sourceField, Expression<Func<TForeign, object>> foreignField)
		{
			string sourceFieldName = ModelDefinition<TSource>.Definition.GetFieldDefinition(sourceField).FieldName;
			string foreignFieldName = ModelDefinition<TForeign>.Definition.GetFieldDefinition(foreignField).FieldName;

			db.AddForeignKey<TSource, TForeign>(sourceField, foreignField, OnFkOption.NoAction, OnFkOption.NoAction, $"{typeof(TSource).TableName()}_{typeof(TForeign).TableName()}_{sourceFieldName}_{foreignFieldName}");
		}

		public static void RemoveForeignKey<TSource, TForeign>(this IDbConnection db, Expression<Func<TSource, object>> sourceField, Expression<Func<TForeign, object>> foreignField)
		{
			string sourceFieldName = ModelDefinition<TSource>.Definition.GetFieldDefinition(sourceField).FieldName;
			string foreignFieldName = ModelDefinition<TForeign>.Definition.GetFieldDefinition(foreignField).FieldName;

			db.DropForeignKey<TSource>($"{typeof(TSource).TableName()}_{typeof(TForeign).TableName()}_{sourceFieldName}_{foreignFieldName}");
		}
	}
}
