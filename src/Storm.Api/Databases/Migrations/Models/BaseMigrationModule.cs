using System.Data;

namespace Storm.Api.Databases.Migrations.Models;

public abstract class BaseMigrationModule : IMigrationModule
{
	public string Name { get; }

	public abstract List<IMigration> Operations { get; }

	protected BaseMigrationModule(string name)
	{
		Name = name;
	}

	public virtual Task StartMigrationOnModule(IDbConnection db)
	{
		return Task.CompletedTask;
	}

	public virtual Task EndMigrationOnModule(IDbConnection db)
	{
		return Task.CompletedTask;
	}
}