using System.Data;
using ServiceStack.OrmLite;
using Storm.Api.Databases.Migrations.Extensions;
using Storm.Api.Databases.Migrations.Models;

namespace Storm.Api.Sample;

internal class Migration001() : BaseMigration(1)
{
	public override async Task Apply(IDbConnection db)
	{
		db.CreateTable<DefaultEntity>();

		await db.InsertAsync(new DefaultEntity
		{
			Name = "Test",
			Description = "Hello world!",
		});

		await db.InsertAsync(new DefaultEntity
		{
			Name = "Test nullable",
			Description = null,
		});
	}
}

internal class Migration002() : BaseMigration(2)
{
	public override Task Apply(IDbConnection db)
	{
		db.AddColumnIfNotExistsWithDefaultValue<DefaultEntity2>(x => x.IsDeleted, "0");
		db.AddColumnIfNotExists<DefaultEntity2>(x => x.EntityDeletedDate!);

		return Task.CompletedTask;
	}
}