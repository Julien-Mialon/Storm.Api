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
	private static TimeProvider _timeProvider = TimeProvider.System;

	public static void Initialize(DatabaseInterceptorDelegate? onInsert, DatabaseInterceptorDelegate? onUpdate, TimeProvider? timeProvider = null)
	{
		_onInsert = onInsert;
		_onUpdate = onUpdate;
		_timeProvider = timeProvider ?? TimeProvider.System;

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
			dateTrackingEntity.MarkAsCreated(_timeProvider);
		}

		_onInsert?.Invoke(command, item);
	}

	private static void OnUpdate(IDbCommand command, object item)
	{
		if (item is ISoftDeleteEntity deletableEntity)
		{
			if (deletableEntity.IsDeleted)
			{
				deletableEntity.MarkAsDeleted(_timeProvider);
			}
			else if (item is IDateTrackingEntity dateTrackingEntity)
			{
				dateTrackingEntity.MarkAsUpdated(_timeProvider);
			}
		}
		else if (item is IDateTrackingEntity dateTrackingEntity)
		{
			dateTrackingEntity.MarkAsUpdated(_timeProvider);
		}

		_onUpdate?.Invoke(command, item);
	}
}