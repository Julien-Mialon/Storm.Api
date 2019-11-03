using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Storm.SqlMigrations
{
	public interface IMigrationModule
	{
		string Name { get; }

		List<IMigration> Operations { get; }

		Task StartMigrationOnModule(IDbConnection db);

		Task EndMigrationOnModule(IDbConnection db);
	}

	public abstract class BaseMigrationModule : IMigrationModule
	{
		protected BaseMigrationModule(string name)
		{
			Name = name;
		}

		public string Name { get; }

		public abstract List<IMigration> Operations { get; }

		public virtual Task StartMigrationOnModule(IDbConnection db)
		{
			return Task.CompletedTask;
		}

		public virtual Task EndMigrationOnModule(IDbConnection db)
		{
			return Task.CompletedTask;
		}
	}
}