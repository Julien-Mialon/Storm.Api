using ServiceStack.DataAnnotations;
using Storm.Api.Databases.Models;

namespace Storm.Api.Sample;

[Alias("DefaultEntities")]
internal class DefaultEntity : BaseEntityWithAutoIncrement
{
	public string Name { get; set; } = string.Empty;

	public string? Description { get; set; }
}

[Alias("DefaultEntities")]
internal class DefaultEntity2 : BaseDeletableEntityWithAutoIncrement
{
	public string Name { get; set; } = string.Empty;

	public string? Description { get; set; }
}