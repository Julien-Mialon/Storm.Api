using ServiceStack.DataAnnotations;

namespace Storm.Api.Databases.Models;

public abstract class BaseDeletableGuidEntity : IGuidEntity, ISoftDeleteEntity, IDateTrackingEntity
{
	[PrimaryKey]
	public Guid Id { get; set; }

	public DateTime EntityCreatedDate { get; set; }

	public DateTime? EntityUpdatedDate { get; set; }

	[Index]
	public bool IsDeleted { get; set; }

	public DateTime? EntityDeletedDate { get; set; }
}