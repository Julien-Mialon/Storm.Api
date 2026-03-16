using System.Data;
using ServiceStack.OrmLite;
using Storm.Api.Databases.Migrations.Models;

namespace Storm.Api.Authentications.Refresh.Database;

/// <summary>
/// Creates the RefreshTokens table.
/// Include this migration in your application's migration module.
/// </summary>
public class RefreshTokenMigration() : BaseMigration(1)
{
	public override Task Apply(IDbConnection db)
	{
		db.CreateTable<RefreshTokenEntity>();
		return Task.CompletedTask;
	}
}