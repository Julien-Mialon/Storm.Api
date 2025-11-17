using ServiceStack.DataAnnotations;

namespace Storm.Api.Databases.Models;

public abstract class BaseGuidEntity : IGuidEntity, IDateTrackingEntity
{
	[PrimaryKey]
	public Guid Id { get; set; }

	public DateTime EntityCreatedDate { get; set; }

	public DateTime? EntityUpdatedDate { get; set; }
}