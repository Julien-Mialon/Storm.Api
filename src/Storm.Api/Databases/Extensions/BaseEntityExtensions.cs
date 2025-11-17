using System.Diagnostics.CodeAnalysis;
using ServiceStack.OrmLite;
using Storm.Api.Databases.Models;

namespace Storm.Api.Databases.Extensions;

public static class BaseEntityExtensions
{
	public static void CopyGenericPropertiesTo(this IDateTrackingEntity storage, IDateTrackingEntity source)
	{
		ArgumentNullException.ThrowIfNull(storage, nameof(storage));
		ArgumentNullException.ThrowIfNull(source, nameof(source));

		storage.EntityCreatedDate = source.EntityCreatedDate;
		storage.EntityUpdatedDate = source.EntityUpdatedDate;
	}

	public static void CopyGenericPropertiesTo(this ISoftDeleteEntity storage, ISoftDeleteEntity source)
	{
		ArgumentNullException.ThrowIfNull(storage, nameof(storage));
		ArgumentNullException.ThrowIfNull(source, nameof(source));

		storage.EntityDeletedDate = source.EntityDeletedDate;
		storage.IsDeleted = source.IsDeleted;
	}

	public static void CopyGenericPropertiesTo(this ILongEntity storage, ILongEntity source)
	{
		ArgumentNullException.ThrowIfNull(storage, nameof(storage));
		ArgumentNullException.ThrowIfNull(source, nameof(source));

		storage.Id = source.Id;
		storage.CollationId = source.CollationId;
	}

	public static void CopyGenericPropertiesFrom(this IGuidEntity storage, IGuidEntity source)
	{
		ArgumentNullException.ThrowIfNull(storage, nameof(storage));
		ArgumentNullException.ThrowIfNull(source, nameof(source));

		storage.Id = source.Id;
	}

	internal static void MarkAsCreated(this IDateTrackingEntity entity)
	{
		entity.EntityCreatedDate = DateTime.UtcNow;
	}

	internal static void MarkAsCreated(this ILongEntity entity)
	{
		if (entity.CollationId == Guid.Empty)
		{
			entity.CollationId = Guid.NewGuid();
		}
	}

	internal static void MarkAsCreated(this IGuidEntity entity)
	{
		if (entity.Id == Guid.Empty)
		{
			entity.Id = Guid.NewGuid();
		}
	}

	internal static void MarkAsUpdated(this IDateTrackingEntity entity)
	{
		entity.EntityUpdatedDate = DateTime.UtcNow;
	}

	internal static void MarkAsDeleted(this ISoftDeleteEntity entity)
	{
		entity.IsDeleted = true;
		entity.EntityDeletedDate = DateTime.UtcNow;
	}

	public static T? NullIfDeleted<T>(this T entity) where T : ISoftDeleteEntity
	{
		if (entity.IsDeleted)
		{
			return default;
		}

		return entity;
	}

	public static async Task<T?> NullIfDeleted<T>(this Task<T> entity) where T : ISoftDeleteEntity => (await entity).NullIfDeleted();

	public static bool IsNotNullOrDefault([NotNullWhen(true)] this IDateTrackingEntity? entity)
	{
		return entity is not null && entity.EntityCreatedDate != default;
	}

	public static bool IsNullOrDefault([NotNullWhen(false)] this IDateTrackingEntity? entity)
	{
		return entity is null || entity.EntityCreatedDate == default;
	}

	public static DateTime LastUpdatedDate(this IDateTrackingEntity entity)
	{
		return entity.EntityUpdatedDate ?? entity.EntityCreatedDate;
	}

	public static SqlExpression<TEntity> NotDeleted<TEntity>(this SqlExpression<TEntity> expression)
		where TEntity : ISoftDeleteEntity
	{
		return expression.Where(x => x.IsDeleted == false);
	}
}