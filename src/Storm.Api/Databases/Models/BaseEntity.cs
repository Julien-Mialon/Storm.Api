using ServiceStack.DataAnnotations;

namespace Storm.Api.Databases.Models;

public abstract class BaseEntity : ILongEntity, IDateTrackingEntity
{
	[PrimaryKey]
	public long Id { get; set; }

	public Guid CollationId { get; set; }

	public DateTime EntityCreatedDate { get; set; }

	public DateTime? EntityUpdatedDate { get; set; }
}