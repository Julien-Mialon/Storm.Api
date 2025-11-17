namespace Storm.Api.Databases.Models;

public interface IDateTrackingEntity
{
	DateTime EntityCreatedDate { get; set; }
	DateTime? EntityUpdatedDate { get; set; }
}