using System.Data;
using ServiceStack.OrmLite;
using Storm.Api.Databases.Connections;
using Storm.Api.Databases.Extensions;
using Storm.Api.Databases.Migrations.Models;
using Storm.Api.Databases.Services;
using Storm.Api.Launchers;
using Migration = Storm.Api.Databases.Migrations.Models.Migration;

namespace Storm.Api.Databases.Migrations;

public class MigrationEngine
{
	private readonly IReadOnlyList<IMigrationModule> _modules;

	public MigrationEngine(IReadOnlyList<IMigrationModule> modules)
	{
		_modules = modules;
	}

	public async Task<bool> Run(IDatabaseService databaseService)
	{
		IDbConnection connection = await databaseService.Connection;

		if (DefaultLauncherOptions.UseOldMigrations)
		{
			connection.CreateTableIfNotExists<OldMigration>();
		}
		else
		{
			connection.CreateTableIfNotExists<Migration>();
		}

		using IDatabaseTransaction transaction = await databaseService.CreateTransaction();
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

				if (DefaultLauncherOptions.UseOldMigrations)
				{
					await connection.InsertAsync(new OldMigration
					{
						Module = module.Name,
						Number = operation.Number,
						EntityCreatedDate = DateTime.UtcNow,
					});
				}
				else
				{
					await connection.InsertAsync(new Migration
					{
						Id = Guid.NewGuid(),
						Module = module.Name,
						Number = operation.Number,
						MigrationDate = DateTime.UtcNow,
					});
				}
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
		if (DefaultLauncherOptions.UseOldMigrations)
		{
			OldMigration? lastMigration = await connection.From<OldMigration>()
				.Where(x => x.Module == moduleName)
				.OrderByDescending(x => x.Number)
				.AsSingleAsync(connection);

			return lastMigration?.Number;
		}
		else
		{
			Migration? lastMigration = await connection.From<Migration>()
				.Where(x => x.Module == moduleName)
				.OrderByDescending(x => x.Number)
				.AsSingleAsync(connection);

			return lastMigration?.Number;
		}
	}
}