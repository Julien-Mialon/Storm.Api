using ServiceStack.OrmLite;
using Storm.Api.Core.Models;

namespace Storm.Api.Core.Databases.Internals;

internal static class SqlFieldsOrdering
{
	public static void Enable()
	{
		OrmLiteConfig.OnFieldDefinitionsInit = ReorderField;
	}

	private static void ReorderField(ModelDefinition definition)
	{
		if (typeof(IEntity).IsAssignableFrom(definition.ModelType))
		{
			FieldDefinition? idField = null;
			FieldDefinition? collationIdField = null;

			List<FieldDefinition?> fields = new(definition.FieldDefinitions.Count)
			{
				null, null //set null for keys space
			};
			foreach (FieldDefinition fieldDefinition in definition.FieldDefinitions)
			{
				switch (fieldDefinition.Name)
				{
					case nameof(IEntity.Id):
						idField = fieldDefinition;
						break;
					case nameof(IEntity.CollationId):
						collationIdField = fieldDefinition;
						break;
					default:
						fields.Add(fieldDefinition);
						break;
				}

			}

			fields[0] = idField;
			fields[1] = collationIdField;

			definition.FieldDefinitions = fields;
		}
		else if (typeof(IGuidEntity).IsAssignableFrom(definition.ModelType))
		{
			FieldDefinition? idField = null;

			List<FieldDefinition?> fields = new(definition.FieldDefinitions.Count)
			{
				null, //set null for keys space
			};
			foreach (FieldDefinition fieldDefinition in definition.FieldDefinitions)
			{
				switch (fieldDefinition.Name)
				{
					case nameof(IGuidEntity.Id):
						idField = fieldDefinition;
						break;

					default:
						fields.Add(fieldDefinition);
						break;
				}

			}

			fields[0] = idField;
			definition.FieldDefinitions = fields;
		}
	}
}