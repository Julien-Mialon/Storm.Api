using ServiceStack.DataAnnotations;

namespace Storm.Api.Core.Models;

public abstract class BaseEntityWithAutoIncrement : IEntity
{
	[PrimaryKey]
	[AutoIncrement]
	public long Id { get; set; }

	public Guid CollationId { get; set; }

	public DateTime EntityCreatedDate { get; set; }

	public DateTime? EntityUpdatedDate { get; set; }

	[Index]
	public bool IsDeleted { get; set; }

	public DateTime? EntityDeletedDate { get; set; }
}