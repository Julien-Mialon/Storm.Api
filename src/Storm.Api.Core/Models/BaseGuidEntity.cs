using ServiceStack.DataAnnotations;

namespace Storm.Api.Core.Models;

public abstract class BaseGuidEntity : IGuidEntity
{
	[PrimaryKey]
	public Guid Id { get; set; }

	[Index]
	public DateTime EntityCreatedDate { get; set; }

	public DateTime? EntityUpdatedDate { get; set; }

	[Index]
	public bool IsDeleted { get; set; }

	public DateTime? EntityDeletedDate { get; set; }
}