using Storm.Api.Databases.Migrations.Models;

namespace Storm.Api.Sample;

internal class MigrationModule() : BaseMigrationModule("MyMigrations")
{
	public override List<IMigration> Operations { get; } =
	[
		new Migration001(),
		new Migration002(),
	];
}