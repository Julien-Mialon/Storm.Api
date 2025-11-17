using System.Data;
using ServiceStack.OrmLite;
using Storm.Api.Databases.Configurations;
using Storm.Api.Databases.Extensions;
using Storm.Api.Databases.Models;

namespace Storm.Api.Databases.Internals;

internal static class OrmLiteInterceptors
{
	private static DatabaseInterceptorDelegate? _onInsert;
	private static DatabaseInterceptorDelegate? _onUpdate;

	public static void Initialize(DatabaseInterceptorDelegate? onInsert, DatabaseInterceptorDelegate? onUpdate)
	{
		_onInsert = onInsert;
		_onUpdate = onUpdate;

		OrmLiteConfig.InsertFilter = OnInsert;
		OrmLiteConfig.UpdateFilter = OnUpdate;
	}

	private static void OnInsert(IDbCommand command, object item)
	{
		if (item is ILongEntity entity)
		{
			entity.MarkAsCreated();
		}
		if (item is IGuidEntity guidEntity)
		{
			guidEntity.MarkAsCreated();
		}
		if (item is IDateTrackingEntity dateTrackingEntity)
		{
			dateTrackingEntity.MarkAsCreated();
		}

		_onInsert?.Invoke(command, item);
	}

	private static void OnUpdate(IDbCommand command, object item)
	{
		if (item is ISoftDeleteEntity deletableEntity)
		{
			if (deletableEntity.IsDeleted)
			{
				deletableEntity.MarkAsDeleted();
			}
			else if (item is IDateTrackingEntity dateTrackingEntity)
			{
				dateTrackingEntity.MarkAsUpdated();
			}
		}
		else if (item is IDateTrackingEntity dateTrackingEntity)
		{
			dateTrackingEntity.MarkAsUpdated();
		}

		_onUpdate?.Invoke(command, item);
	}
}