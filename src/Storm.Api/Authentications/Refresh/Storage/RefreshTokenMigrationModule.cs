using Storm.Api.Databases.Migrations.Models;

namespace Storm.Api.Authentications.Refresh.Storage;

public class RefreshTokenMigrationModule() : BaseMigrationModule("Storm.Api.Authentications.Refresh")
{
	public override List<IMigration> Operations { get; } =
	[
		new RefreshTokenMigration01()
	];
}