using System.Data;
using ServiceStack.OrmLite;
using Storm.Api.Core.Extensions;
using Storm.Api.Core.Models;

namespace Storm.Api.Core.Databases.Internals;

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
		if (item is IEntity entity)
		{
			entity.MarkAsCreated();
		}
		else if (item is IGuidEntity guidEntity)
		{
			guidEntity.MarkAsCreated();
		}
		else if (item is ICommonEntity commonEntity)
		{
			commonEntity.MarkAsCreated();
		}

		_onInsert?.Invoke(command, item);
	}

	private static void OnUpdate(IDbCommand command, object item)
	{
		if (item is ICommonEntity entity)
		{
			if (entity.IsDeleted)
			{
				entity.MarkAsDeleted();
			}
			else
			{
				entity.MarkAsUpdated();
			}
		}

		_onUpdate?.Invoke(command, item);
	}
}