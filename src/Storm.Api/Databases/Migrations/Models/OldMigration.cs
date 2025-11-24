using ServiceStack.DataAnnotations;
using Storm.Api.Databases.Models;

namespace Storm.Api.Databases.Migrations.Models;

[Alias("Migrations")]
public class OldMigration : BaseDeletableEntityWithAutoIncrement
{
	[Index]
	public int Number { get; set; }

	[Index]
	[StringLength(200)]
	public string? Module { get; set; }
}