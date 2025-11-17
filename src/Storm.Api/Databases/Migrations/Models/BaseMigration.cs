using System.Data;

namespace Storm.Api.Databases.Migrations.Models;

public abstract class BaseMigration : IMigration
{
	public int Number { get; }

	protected BaseMigration(int number)
	{
		Number = number;
	}

	public abstract Task Apply(IDbConnection db);
}