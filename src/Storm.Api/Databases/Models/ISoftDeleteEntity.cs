namespace Storm.Api.Databases.Models;

public interface ISoftDeleteEntity
{
	bool IsDeleted { get; set; }
	DateTime? EntityDeletedDate { get; set; }
}