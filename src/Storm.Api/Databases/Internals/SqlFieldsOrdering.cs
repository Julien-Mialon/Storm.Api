using ServiceStack.OrmLite;
using Storm.Api.Databases.Models;

namespace Storm.Api.Databases.Internals;

internal static class SqlFieldsOrdering
{
	public static void Enable()
	{
		OrmLiteConfig.OnModelDefinitionInit = ReorderField;
	}

	private static void ReorderField(ModelDefinition definition)
	{
		FieldDefinition? idField = null;
		FieldDefinition? collationIdField = null;
		FieldDefinition? isDeletedField = null;
		FieldDefinition? entityDeletedDateField = null;
		FieldDefinition? entityCreatedDateField = null;
		FieldDefinition? entityUpdatedDateField = null;

		List<FieldDefinition> fields = new(definition.FieldDefinitions.Count);

		foreach (FieldDefinition fieldDefinition in definition.FieldDefinitions)
		{
			switch (fieldDefinition.Name)
			{
				case nameof(ILongEntity.Id):
					// case nameof(IGuidEntity.Id):
					idField = fieldDefinition;
					break;
				case nameof(ILongEntity.CollationId):
					collationIdField = fieldDefinition;
					break;
				case nameof(ISoftDeleteEntity.IsDeleted):
					isDeletedField = fieldDefinition;
					break;
				case nameof(ISoftDeleteEntity.EntityDeletedDate):
					entityDeletedDateField = fieldDefinition;
					break;
				case nameof(IDateTrackingEntity.EntityCreatedDate):
					entityCreatedDateField = fieldDefinition;
					break;
				case nameof(IDateTrackingEntity.EntityUpdatedDate):
					entityUpdatedDateField = fieldDefinition;
					break;
				default:
					fields.Add(fieldDefinition);
					break;
			}
		}

		IEnumerable<FieldDefinition> result = fields;
		if(collationIdField is not null)
		{
			result = result.Prepend(collationIdField);
		}
		if (idField is not null)
		{
			result = result.Prepend(idField);
		}
		if (isDeletedField is not null)
		{
			result = result.Append(isDeletedField);
		}
		if (entityCreatedDateField is not null)
		{
			result = result.Append(entityCreatedDateField);
		}
		if (entityUpdatedDateField is not null)
		{
			result = result.Append(entityUpdatedDateField);
		}
		if (entityDeletedDateField is not null)
		{
			result = result.Append(entityDeletedDateField);
		}

		definition.FieldDefinitions = result.ToList();

		OrmLiteConfig.OnModelDefinitionInit = null;
		definition.AfterInit();
		OrmLiteConfig.OnModelDefinitionInit = ReorderField;
	}
}