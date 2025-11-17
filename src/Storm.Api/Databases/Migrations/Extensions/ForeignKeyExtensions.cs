using System.Data;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Storm.Api.Databases.Extensions;

namespace Storm.Api.Databases.Migrations.Extensions;

public static class ForeignKeyExtensions
{
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
}