using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack.OrmLite;
using Storm.Api.Core;
using Storm.Api.Core.Databases;
using Storm.Api.Core.Extensions;

namespace Storm.SqlMigrations
{
	public class MigrationEngine : BaseServiceContainer
	{
		private readonly List<IMigrationModule> _modules;

		public MigrationEngine(IServiceProvider services, List<IMigrationModule> modules) : base(services)
		{
			_modules = modules;
		}

		public async Task<bool> Run()
		{
			using (IDatabaseService databaseService = Resolve<IDatabaseService>())
			{
				IDbConnection connection = await databaseService.Connection;

				connection.CreateTableIfNotExists<Migration>();

				using (IDatabaseTransaction transaction = await databaseService.Transaction())
				{
					try
					{
						foreach (IMigrationModule module in _modules)
						{
							if (!await MigrateModule(connection, module))
							{
								return false;
							}
						}
						transaction.Commit();
					}
					catch (Exception)
					{
						transaction.Rollback();
						throw;
					}
				}
			}

			return true;
		}

		private static async Task<bool> MigrateModule(IDbConnection connection, IMigrationModule module)
		{
			int lastAppliedMigration = await GetLastAppliedMigration(connection, module.Name) ?? -1;
			List<IMigration> migrationsToApply = module.Operations.OrderBy(x => x.Number).SkipWhile(x => x.Number <= lastAppliedMigration).ToList();

			if (migrationsToApply.Count == 0)
			{
				return true;
			}

			try
			{
				await module.StartMigrationOnModule(connection);
				foreach (IMigration operation in migrationsToApply)
				{
					await operation.Apply(connection);

					await connection.InsertAsync(new Migration
					{
						Module = module.Name,
						Number = operation.Number,
					});
				}

				await module.EndMigrationOnModule(connection);
			}
			catch (Exception exception)
			{
				throw new InvalidOperationException($"Error while applying migrations to module {module.Name} from migration {lastAppliedMigration}", exception);
			}

			return true;
		}

		private static async Task<int?> GetLastAppliedMigration(IDbConnection connection, string moduleName)
		{
			Migration lastMigration = await connection.From<Migration>()
							.Where(x => x.Module == moduleName)
							.OrderByDescending(x => x.Number)
							.AsSingleAsync(connection);

			return lastMigration?.Number;
		}
	}
}