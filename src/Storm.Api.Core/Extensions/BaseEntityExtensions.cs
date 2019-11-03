using System;
using System.Threading.Tasks;
using Storm.Api.Core.Models;

namespace Storm.Api.Core.Extensions
{
	public static class BaseEntityExtensions
	{
		public static void CopyGenericPropertiesFrom(this BaseEntity storage, BaseEntity source)
		{
			if (storage is null)
			{
				throw new NullReferenceException();
			}

			if (source is null)
			{
				throw new ArgumentNullException(nameof(source), "Source should not be null");
			}

			storage.Id = source.Id;
			storage.CollationId = source.CollationId;
			storage.EntityCreatedDate = source.EntityCreatedDate;
			storage.EntityUpdatedDate = source.EntityUpdatedDate;
			storage.EntityDeletedDate = source.EntityDeletedDate;
			storage.IsDeleted = source.IsDeleted;
		}

		internal static void MarkAsCreated(this BaseEntity entity)
		{
			entity.EntityCreatedDate = DateTime.UtcNow;
		}

		internal static void MarkAsUpdated(this BaseEntity entity)
		{
			entity.EntityUpdatedDate = DateTime.UtcNow;
		}

		internal static void MarkAsDeleted(this BaseEntity entity)
		{
			entity.IsDeleted = true;
			entity.EntityDeletedDate = DateTime.UtcNow;
		}

		public static T NullIfDeleted<T>(this T entity) where T : BaseEntity
		{
			if (entity.IsDeleted)
			{
				return null;
			}

			return entity;
		}

		public static async Task<T> NullIfDeleted<T>(this Task<T> entity) where T : BaseEntity => (await entity).NullIfDeleted();

		public static bool IsNotNullOrDefault(this BaseEntity entity)
		{
			return entity != null && entity.EntityCreatedDate != default;
		}

		public static bool IsNullOrDefault(this BaseEntity entity)
		{
			return entity is null || entity.EntityCreatedDate == default;
		}

		public static DateTime LastUpdatedDate(this BaseEntity entity)
		{
			return entity.EntityUpdatedDate ?? entity.EntityCreatedDate;
		}
	}
}