using System.Data;
using ServiceStack.OrmLite;
using Storm.Api.Core.Extensions;
using Storm.Api.Core.Models;

namespace Storm.Api.Core.Databases.Internals
{
	internal static class OrmLiteInterceptors
	{
		public static void Initialize()
		{
			OrmLiteConfig.InsertFilter = OnInsert;
			OrmLiteConfig.UpdateFilter = OnUpdate;
		}

		private static void OnInsert(IDbCommand command, object item)
		{
			if (item is IEntity entity)
			{
				entity.MarkAsCreated();
			}
		}

		private static void OnUpdate(IDbCommand command, object item)
		{
			if (item is IEntity entity)
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
		}
	}
}