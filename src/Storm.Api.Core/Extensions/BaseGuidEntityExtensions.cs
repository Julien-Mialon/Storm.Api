using Storm.Api.Core.Models;

namespace Storm.Api.Core.Extensions;

public static class BaseGuidEntityExtensions
{
	public static void CopyGenericPropertiesFrom(this IGuidEntity storage, IGuidEntity source)
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
		storage.EntityCreatedDate = source.EntityCreatedDate;
		storage.EntityUpdatedDate = source.EntityUpdatedDate;
		storage.EntityDeletedDate = source.EntityDeletedDate;
		storage.IsDeleted = source.IsDeleted;
	}

	internal static void MarkAsCreated(this IGuidEntity entity)
	{
		if (entity.Id == default)
		{
			entity.Id = Guid.NewGuid();
		}

		entity.EntityCreatedDate = DateTime.UtcNow;
	}
}