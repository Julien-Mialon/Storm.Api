using ServiceStack.DataAnnotations;

namespace Storm.Api.Databases.Models;

public abstract class BaseEntityWithAutoIncrement : ILongEntity, IDateTrackingEntity
{
	[PrimaryKey]
	[AutoIncrement]
	public long Id { get; set; }

	public Guid CollationId { get; set; }

	public DateTime EntityCreatedDate { get; set; }

	public DateTime? EntityUpdatedDate { get; set; }
}