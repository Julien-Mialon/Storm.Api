namespace Storm.Api.Core.Models;

public interface ICommonEntity
{
	DateTime EntityCreatedDate { get; set; }
	DateTime? EntityUpdatedDate { get; set; }
	bool IsDeleted { get; set; }
	DateTime? EntityDeletedDate { get; set; }
}