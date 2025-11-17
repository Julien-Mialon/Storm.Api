using System.Data;
using System.Linq.Expressions;
using ServiceStack.OrmLite;

namespace Storm.Api.Databases.Migrations.Extensions;

public static class SqlServerColumnExtensions
{
	public static bool AddColumnIfNotExistsWithDefaultValue<T>(this IDbConnection db, Expression<Func<T, object>> field, string defaultSubstituteValue)
	{
		if (db.ColumnExists(field))
		{
			return false;
		}

		ModelDefinition? modelDefinition = ModelDefinition<T>.Definition;
		FieldDefinition? fieldDefinition = modelDefinition.GetFieldDefinition(field);
		if (!string.IsNullOrEmpty(fieldDefinition.DefaultValue))
		{
			db.AddColumn<T>(field);
		}
		else
		{
			FieldDefinition fieldDefinitionCopy = fieldDefinition.Clone();
			fieldDefinitionCopy.DefaultValue = defaultSubstituteValue;
			fieldDefinitionCopy.DefaultValueConstraint = $"DF_{modelDefinition.ModelName}_{fieldDefinition.FieldName}";

			db.AddColumn(typeof(T), fieldDefinitionCopy);
			db.DropConstraint(typeof(T), fieldDefinitionCopy.DefaultValueConstraint);
		}
		return true;
	}
}