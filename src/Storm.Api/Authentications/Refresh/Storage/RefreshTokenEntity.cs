using ServiceStack.DataAnnotations;
using Storm.Api.Databases.Models;

namespace Storm.Api.Authentications.Refresh.Storage;

[Alias("RefreshTokens")]
public class RefreshTokenEntity : BaseGuidEntity
{
	[Index]
	public Guid AccountId { get; set; }

	[Index]
	[StringLength(64)]
	public string Jti { get; set; } = "";

	public DateTime ExpiresAt { get; set; }
}