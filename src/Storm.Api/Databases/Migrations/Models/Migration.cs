using ServiceStack.DataAnnotations;

namespace Storm.Api.Databases.Migrations.Models;

[Alias("Migrations")]
public class Migration
{
	public Guid Id { get; set; }

	[Index]
	public int Number { get; set; }

	[Index]
	[StringLength(200)]
	public string? Module { get; set; }

	public DateTime MigrationDate { get; set; }
}