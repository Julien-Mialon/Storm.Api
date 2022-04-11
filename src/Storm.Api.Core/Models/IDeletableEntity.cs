namespace Storm.Api.Core.Models;

public interface IDeletableEntity : ICommonEntity
{
	bool IsDeleted { get; set; }
	DateTime? EntityDeletedDate { get; set; }
}