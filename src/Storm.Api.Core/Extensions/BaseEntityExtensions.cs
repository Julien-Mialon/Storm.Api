using ServiceStack.OrmLite;
using Storm.Api.Core.Models;

namespace Storm.Api.Core.Extensions;

public static class BaseEntityExtensions
{
	public static void CopyGenericPropertiesFrom(this ICommonEntity storage, ICommonEntity source)
	{
		if (storage is null)
		{
			throw new NullReferenceException();
		}

		if (source is null)
		{
			throw new ArgumentNullException(nameof(source), "Source should not be null");
		}

		storage.EntityCreatedDate = source.EntityCreatedDate;
		storage.EntityUpdatedDate = source.EntityUpdatedDate;
		storage.EntityDeletedDate = source.EntityDeletedDate;
		storage.IsDeleted = source.IsDeleted;
	}

	public static void CopyGenericPropertiesFrom(this IEntity storage, IEntity source)
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

	internal static void MarkAsCreated(this ICommonEntity entity)
	{
		entity.EntityCreatedDate = DateTime.UtcNow;
	}

	internal static void MarkAsCreated(this IEntity entity)
	{
		if (entity.CollationId == default)
		{
			entity.CollationId = Guid.NewGuid();
		}

		((ICommonEntity)entity).MarkAsCreated();
	}

	internal static void MarkAsUpdated(this ICommonEntity entity)
	{
		entity.EntityUpdatedDate = DateTime.UtcNow;
	}

	internal static void MarkAsDeleted(this ICommonEntity entity)
	{
		entity.IsDeleted = true;
		entity.EntityDeletedDate = DateTime.UtcNow;
	}

	public static T? NullIfDeleted<T>(this T entity) where T : ICommonEntity
	{
		if (entity.IsDeleted)
		{
			return default;
		}

		return entity;
	}

	public static async Task<T?> NullIfDeleted<T>(this Task<T> entity) where T : ICommonEntity => (await entity).NullIfDeleted();

	public static bool IsNotNullOrDefault(this ICommonEntity? entity)
	{
		return entity is not null && entity.EntityCreatedDate != default;
	}

	public static bool IsNullOrDefault(this ICommonEntity? entity)
	{
		return entity is null || entity.EntityCreatedDate == default;
	}

	public static DateTime LastUpdatedDate(this ICommonEntity entity)
	{
		return entity.EntityUpdatedDate ?? entity.EntityCreatedDate;
	}

	public static SqlExpression<TEntity> NotDeleted<TEntity>(this SqlExpression<TEntity> expression)
		where TEntity : ICommonEntity
	{
		return expression.Where(x => x.IsDeleted == false);
	}
}