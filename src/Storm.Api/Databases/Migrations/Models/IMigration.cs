using System.Data;

namespace Storm.Api.Databases.Migrations.Models;

public interface IMigration
{
	int Number { get; }

	Task Apply(IDbConnection db);
}