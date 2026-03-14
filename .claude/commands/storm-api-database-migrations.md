You are helping write a database migration using the **Storm.Api** framework. Follow all patterns below exactly.

---

## Key Rule

**Never modify an existing migration.** Once a migration has run it is recorded and will never re-run. Always add a new migration with the next number.

---

## BaseMigration

Each migration is a numbered class extending `BaseMigration`. Migrations run in ascending order by number.

```csharp
internal class Migration001() : BaseMigration(1)
{
    public override async Task Apply(IDbConnection db)
    {
        db.CreateTable<UserEntity>();

        await db.InsertAsync(new UserEntity
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            Name = "Admin",
        });
    }
}
```

Common OrmLite operations inside `Apply`:

```csharp
// Create a table (idempotent — only creates if it doesn't exist)
db.CreateTable<MyEntity>();

// Add a nullable column
db.AddColumnIfNotExists<MyEntity>(x => x.NewColumn);

// Add a NOT NULL column with a default value for existing rows
db.AddColumnIfNotExistsWithDefaultValue<MyEntity>(x => x.IsActive, "1");

// Drop a column
db.DropColumnIfExists<MyEntity>("OldColumn");

// Check existence
bool exists = db.ColumnExists<MyEntity>("SomeColumn");

// Seed data
await db.InsertAsync(new MyEntity { ... });
await db.UpdateAsync<MyEntity>(new { Status = "active" }, x => x.Status == null);
```

---

## BaseMigrationModule

Group all migrations for a logical area into a module:

```csharp
internal class AppMigrationModule() : BaseMigrationModule("App")
{
    public override List<IMigration> Operations { get; } =
    [
        new Migration001(),
        new Migration002(),
        new Migration003(),
    ];
}
```

- The module `Name` is used to namespace migrations in the tracking table — use a stable, meaningful string
- Add new migrations to the end of the list; never reorder existing ones

---

## Startup Registration

Register modules in the `BaseStartup` constructor (not in `ConfigureServices`):

```csharp
public class Startup(IConfiguration configuration, IWebHostEnvironment env)
    : BaseStartup(configuration, env)
{
    public Startup(...) : base(...)
    {
        UseMigrationModules(new AppMigrationModule());

        // Optionally block HTTP traffic until migrations finish:
        // WaitForMigrationsBeforeStarting = true;
    }
}
```

`WaitForMigrationsBeforeStarting` defaults to `false` (migrations run in the background). Set it to `true` when your app cannot tolerate a missing column/table on startup.

---

## Anti-Patterns to Avoid

| ❌ Wrong | ✅ Correct |
|---|---|
| Edit an existing migration | Add a new migration with the next number |
| Reorder migrations in the module list | Always append to the end |
| Use EF migrations | Use `BaseMigration` + OrmLite |
