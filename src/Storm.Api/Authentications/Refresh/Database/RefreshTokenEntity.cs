using ServiceStack.DataAnnotations;
using Storm.Api.Databases.Models;

namespace Storm.Api.Authentications.Refresh.Database;

[Alias("RefreshTokens")]
public class RefreshTokenEntity : IGuidEntity, IDateTrackingEntity
{
	[PrimaryKey]
	public Guid Id { get; set; }

	[Index]
	public Guid AccountId { get; set; }

	[Index]
	[StringLength(64)]
	public string Jti { get; set; } = "";

	public DateTime ExpiresAt { get; set; }

	public DateTime EntityCreatedDate { get; set; }

	public DateTime? EntityUpdatedDate { get; set; }
}