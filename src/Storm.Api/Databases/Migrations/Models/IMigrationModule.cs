using System.Data;

namespace Storm.Api.Databases.Migrations.Models;

public interface IMigrationModule
{
	string Name { get; }

	List<IMigration> Operations { get; }

	Task StartMigrationOnModule(IDbConnection db);

	Task EndMigrationOnModule(IDbConnection db);
}