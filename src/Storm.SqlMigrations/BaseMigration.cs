using System.Data;

namespace Storm.SqlMigrations;

public interface IMigration
{
	int Number { get; }

	Task Apply(IDbConnection db);
}

public abstract class BaseMigration : IMigration
{
	public int Number { get; }

	protected BaseMigration(int number)
	{
		Number = number;
	}

	public abstract Task Apply(IDbConnection db);
}