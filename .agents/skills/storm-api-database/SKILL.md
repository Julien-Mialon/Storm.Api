---
name: storm-api-database
description: Implement database entities and repositories using the Storm.Api framework with ServiceStack.OrmLite. Use when creating entities, repositories, or database queries.
user-invocable: true
disable-model-invocation: false
---

You are helping implement database entities and repositories using the **Storm.Api** framework (ServiceStack.OrmLite). Follow all patterns below exactly. For global rules (logging, extensions, anti-patterns), see `/storm-api`.

**Never use Entity Framework or Dapper.** OrmLite is the only ORM.

The user's request: $ARGUMENTS

---

## Database Connection Methods

All connection methods are available on `BaseAction` / `BaseDatabaseService`:

```csharp
// Read
var entity = await UseReadConnection(db => db.SingleByIdAsync<MyEntity>(id));
var list   = await UseReadConnection(db => db.SelectAsync<MyEntity>());
var where  = await UseReadConnection(db => db.SelectAsync<MyEntity>(x => x.Active));

// Write
await UseWriteConnection(db => db.InsertAsync(entity));
await UseWriteConnection(db => db.UpdateAsync(entity));
await UseWriteConnection(db => db.DeleteByIdAsync<MyEntity>(id));

// Transaction
await WithDatabaseTransaction(async (db, tx) =>
{
    await db.InsertAsync(entityA);
    await db.InsertAsync(entityB);
    return Unit.Default;
});
```

| Method | Use for |
|--------|---------|
| `UseConnection<T>()` | Any operation |
| `UseReadConnection<T>()` | Read-only queries |
| `UseWriteConnection<T>()` | Inserts, updates, deletes |
| `WithDatabaseTransaction<T>()` | Multi-step writes (auto commit/rollback) |

---

## Entity Base Classes

Choose the base class that matches your primary key type and whether you need soft delete.

### Guid primary key

```csharp
// No soft delete
[Alias("users")]
public class UserEntity : BaseGuidEntity
{
    [Index]
    public required string Email { get; set; }
    public required string Name { get; set; }
}

// With soft delete (Never call db.Delete on it, always set IsDeleted = true instead)
[Alias("posts")]
public class PostEntity : BaseDeletableGuidEntity
{
    public required string Title { get; set; }
}
```

### Long primary key (auto-increment)

```csharp
// No soft delete
[Alias("products")]
public class ProductEntity : BaseEntityWithAutoIncrement
{
    public required string Sku { get; set; }
}

// With soft delete
[Alias("orders")]
public class OrderEntity : BaseDeletableEntityWithAutoIncrement
{
    public required string Reference { get; set; }
}
```

### Common OrmLite attributes

| Attribute | Purpose |
|-----------|---------|
| `[Alias("table_name")]` | Map class to a specific table name |
| `[Index]` | Create a database index on the column |
| `[Unique]` | Create a unique index |
| `[Required]` | NOT NULL constraint |
| `[Default("value")]` | Column default value |
| `[StringLength(400)]` | Maximum length for a string column |

### What each base class provides

| Base class | PK type | Auto-increment | Soft delete | Interfaces |
|------------|---------|---------------|-------------|------------|
| `BaseGuidEntity` | `Guid` | No | No | `IGuidEntity`, `IDateTrackingEntity` |
| `BaseDeletableGuidEntity` | `Guid` | No | Yes | `IGuidEntity`, `ISoftDeleteEntity`, `IDateTrackingEntity` |
| `BaseEntityWithAutoIncrement` | `long` | Yes | No | `ILongEntity`, `IDateTrackingEntity` |
| `BaseDeletableEntityWithAutoIncrement` | `long` | Yes | Yes | `ILongEntity`, `ISoftDeleteEntity`, `IDateTrackingEntity` |

All date-tracking entities automatically get `EntityCreatedDate` and `EntityUpdatedDate` columns. Soft-delete entities get `IsDeleted` (indexed) and `EntityDeletedDate`.

---

## Repositories

Repositories wrap common CRUD operations. The correct implementation is chosen automatically based on whether the entity implements `ISoftDeleteEntity`.

### Define

```csharp
// Guid-keyed entity
public class UserRepository(IServiceProvider services) : BaseGuidRepository<UserEntity>(services) { }

// Long-keyed entity
public class ProductRepository(IServiceProvider services) : BaseLongRepository<ProductEntity>(services) { }
```

### Register in DI

```csharp
// In Startup.ConfigureServices:
services.AddRepository<UserEntity, UserRepository>();         // Guid
services.AddLongRepository<ProductEntity, ProductRepository>(); // Long
```

### Use inside an action

```csharp
var repo = Resolve<UserRepository>();

var user  = await repo.GetById(id);           // returns TEntity?
var all   = await repo.List();                 // returns List<TEntity> (excludes soft-deleted)
var created = await repo.Create(newUser);      // returns the saved entity
await repo.Update(user);
await repo.Delete(id);                         // soft-delete if ISoftDeleteEntity, hard-delete otherwise
```

---

## Anti-Patterns to Avoid

| ❌ Wrong | ✅ Correct |
|---|---|
| Entity Framework / Dapper | ServiceStack.OrmLite |
| `context.Database.OpenConnection()` | `UseConnection(db => ...)` |
| `services.AddScoped<UserRepository>()` alone | `services.AddRepository<UserEntity, UserRepository>()` |
