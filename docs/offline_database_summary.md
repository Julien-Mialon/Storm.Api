# Offline-database behaviour summary

## Scope

This document captures findings gathered while preparing the design for read-replica support in Storm.Api. The target failover model is:

- Primary handles all writes and (today) all reads.
- Read replicas are strictly read-only mirrors of the primary.
- When the primary becomes unavailable (network, server), only replicas respond until one is manually promoted to primary.

The question this document answers is: **given the code as it exists today, what would still function and what would fail during a period where only the read replicas are reachable?** It is descriptive, not prescriptive — it documents current behaviour and the constraints any future design must work within. No new APIs are proposed here.

## Current connection model

All three intent-declaring helpers on `BaseDatabaseService` are currently aliases for the same code path:

- `UseConnection` — `src/Storm.Api/Databases/Services/BaseDatabaseService.cs:15–25`
- `UseReadConnection` — `BaseDatabaseService.cs:27–37`
- `UseWriteConnection` — `BaseDatabaseService.cs:39–49`

Each one awaits `DatabaseService.Connection` (`src/Storm.Api/Databases/Services/DatabaseService.cs:15`) and hands the `IDbConnection` to the caller. The method names carry intent for future routing, but no routing exists yet — every call ends up on the same connection.

That single connection is held by the scoped `DatabaseService`:

- Lazy open and cache — `DatabaseService.cs:22–32`. First access calls `_factory.Open(ct)` on `IDatabaseConnectionFactory` and memoises the task.
- Disposed at end of request — `src/Storm.Api/Databases/Middlewares/DisposeConnectionMiddleware.cs:9–20`.

Transactions are bound to whichever connection `DatabaseService.Connection` returned:

- `DatabaseService.CreateTransaction` — `DatabaseService.cs:34–45`. It awaits `Connection` and calls `connection.OpenTransaction(...)`. If a transaction is already live on the service it returns a `DummyTransaction` instead (no-op `Commit`/`Rollback`, same connection) — see `DatabaseService.cs:37–40` and `src/Storm.Api/Databases/Connections/DummyTransaction.cs:5–19`.
- `DatabaseTransaction` holds its connection in a read-only field — `src/Storm.Api/Databases/Connections/DatabaseTransaction.cs:13–19`.

`BaseDatabaseService.WithDatabaseTransaction` — `BaseDatabaseService.cs:51–73` — delegates to `IDatabaseService.InTransaction` and forwards `transaction.Connection` to the executor. Any reads made inside the transaction share that same connection.

Implication for a future read/write split: `UseReadConnection` and `UseWriteConnection` have to diverge (different connections, possibly different factories). The `DatabaseService`'s single-connection cache either needs to become a pair of caches or be replaced entirely. Transactions must stay on the primary, and any `UseReadConnection` call made while a write transaction is active has to fall back to the primary connection to avoid reading stale replica state.

## Write audit (framework-owned writes)

All framework-owned writes currently route through `UseWriteConnection` or `WithDatabaseTransaction`. No write path in `src/` bypasses `BaseDatabaseService` except by design (see the bypass audit).

Repositories — `src/Storm.Api/Databases/Repositories/`:

| File | Method | Line | Helper |
|---|---|---|---|
| `BaseDeletableGuidRepository.cs` | `Create(TEntity)` | 53–60 | `UseWriteConnection` |
| `BaseDeletableGuidRepository.cs` | `Create(List<TEntity>)` | 62–68 | `UseWriteConnection` |
| `BaseDeletableGuidRepository.cs` | `Update(TEntity)` | 70–77 | `UseWriteConnection` |
| `BaseDeletableGuidRepository.cs` | `Update(List<TEntity>)` | 79–85 | `UseWriteConnection` |
| `BaseDeletableGuidRepository.cs` | `Delete(Guid)` (soft) | 87–100 | `UseWriteConnection` |
| `BaseDeletableGuidRepository.cs` | `Delete(TEntity)` (soft) | 102–110 | `UseWriteConnection` |
| `BaseDeletableLongRepository.cs` | `Create(TEntity)` | 53–60 | `UseWriteConnection` |
| `BaseDeletableLongRepository.cs` | `Create(List<TEntity>)` | 62–68 | `UseWriteConnection` |
| `BaseDeletableLongRepository.cs` | `Update(TEntity)` | 70–77 | `UseWriteConnection` |
| `BaseDeletableLongRepository.cs` | `Update(List<TEntity>)` | 79–85 | `UseWriteConnection` |
| `BaseDeletableLongRepository.cs` | `Delete(long)` (soft) | 87–100 | `UseWriteConnection` |
| `BaseDeletableLongRepository.cs` | `Delete(TEntity)` (soft) | 102–110 | `UseWriteConnection` |
| `BaseNonDeletableGuidRepository.cs` | `Create(TEntity)` | 50–57 | `UseWriteConnection` |
| `BaseNonDeletableGuidRepository.cs` | `Create(List<TEntity>)` | 59–65 | `UseWriteConnection` |
| `BaseNonDeletableGuidRepository.cs` | `Update(TEntity)` | 67–74 | `UseWriteConnection` |
| `BaseNonDeletableGuidRepository.cs` | `Update(List<TEntity>)` | 76–82 | `UseWriteConnection` |
| `BaseNonDeletableGuidRepository.cs` | `Delete(Guid)` | 84–91 | `UseWriteConnection` |
| `BaseNonDeletableGuidRepository.cs` | `Delete(TEntity)` | 93–100 | `UseWriteConnection` |
| `BaseNonDeletableLongRepository.cs` | `Create(TEntity)` | 50–57 | `UseWriteConnection` |
| `BaseNonDeletableLongRepository.cs` | `Create(List<TEntity>)` | 59–65 | `UseWriteConnection` |
| `BaseNonDeletableLongRepository.cs` | `Update(TEntity)` | 67–74 | `UseWriteConnection` |
| `BaseNonDeletableLongRepository.cs` | `Update(List<TEntity>)` | 76–82 | `UseWriteConnection` |
| `BaseNonDeletableLongRepository.cs` | `Delete(long)` | 84–91 | `UseWriteConnection` |
| `BaseNonDeletableLongRepository.cs` | `Delete(TEntity)` | 93–100 | `UseWriteConnection` |

Refresh-token database storage — `src/Storm.Api/Authentications/Refresh/Storage/DatabaseRefreshTokenStorage.cs`:

| Method | Line | Helper | Operation |
|---|---|---|---|
| `StoreAsync` | 13–25 | `UseWriteConnection` | `INSERT` into `RefreshTokens` |
| `RotateAsync` | 38–58 | `UseWriteConnection` | `DELETE` old + `INSERT` new |
| `RevokeAsync` | 60–66 | `UseWriteConnection` | `DELETE` by `jti` |
| `RevokeAllForAccountAsync` | 68–74 | `UseWriteConnection` | `DELETE` by `accountId` |

Transactional orchestration:

- `BaseRefreshAction.Action` — `src/Storm.Api/Authentications/Refresh/BaseRefreshAction.cs:67–80`. Wraps `storage.RotateAsync` and the transport `EmitToken` call in `WithDatabaseTransaction`. Note that `storage.ValidateAsync` at line 41 runs **outside** the transaction as an early cheap check.

Conclusion: every write in the framework proper already declares intent correctly. No write uses `UseConnection` or `UseReadConnection`. Nothing writes to the database without going through one of the intent helpers.

## Bypass audit (writes that do not go through `BaseDatabaseService`)

These are intentional. They all target the primary, and any read-replica design must make sure they continue to do so.

1. **User-defined migrations** receive `IDbConnection` directly on `IMigration.Apply(IDbConnection db)`. Example writes:
   - `sample/Storm.Api.Sample/Migration001.cs:12` — `db.CreateTable<DefaultEntity>()`
   - `sample/Storm.Api.Sample/Migration001.cs:14–24` — `db.InsertAsync(...)`
   - `sample/Storm.Api.Sample/Migration001.cs:32–33` — `db.AddColumnIfNotExistsWithDefaultValue`, `db.AddColumnIfNotExists`
   - `src/Storm.Api/Authentications/Refresh/Storage/RefreshTokenMigration01.cs:15` — `db.CreateTable<RefreshTokenEntity>()`

2. **`MigrationEngine` internals** touch the connection directly to bootstrap the migration bookkeeping table and record applied migrations — `src/Storm.Api/Databases/Migrations/MigrationEngine.cs`:
   - `25` — `IDbConnection connection = await databaseService.Connection;`
   - `29`, `33` — `connection.CreateTableIfNotExists<OldMigration>()` / `CreateTableIfNotExists<Migration>()`
   - `36` — `await databaseService.CreateTransaction()` wraps the whole migration run
   - `76`, `85` — `await connection.InsertAsync(new OldMigration { ... })` / `new Migration { ... }` after each applied migration
   - `109–123` — reads migration history via `connection.From<...>().Where(...).AsSingleAsync`

3. **`OrmLiteInterceptors`** — `src/Storm.Api/Databases/Internals/OrmLiteInterceptors.cs:25–62`. The built-in `OnInsert` / `OnUpdate` only mutate entity properties in memory (`MarkAsCreated`, `MarkAsUpdated`, `MarkAsDeleted`). They invoke user-provided `_onInsert` / `_onUpdate` delegates at lines 40 and 61; those delegates could in principle write through the same `IDbCommand`, but that is out of framework scope.

4. **`LogServiceDatabaseLog`** — `src/Storm.Api/Databases/LogServiceDatabaseLog.cs`. Despite the name, this is the OrmLite-side `ILog` adapter that forwards SQL log events into `ILogService` (line 67–97). It does not itself write to the database. The configured `ILogService` sink (Elasticsearch / Serilog / Console, wired in `src/Storm.Api/Launchers/BaseStartup.cs:170–210`) decides whether logs end up in a database at all — outside the scope of this analysis.

## Read audit (framework-owned reads)

| File | Method | Line | Helper |
|---|---|---|---|
| `BaseDeletableGuidRepository.cs` | `ExistsById` | 21–30 | `UseReadConnection` |
| `BaseDeletableGuidRepository.cs` | `GetById` | 32–41 | `UseReadConnection` |
| `BaseDeletableGuidRepository.cs` | `List` | 43–51 | `UseReadConnection` |
| `BaseDeletableLongRepository.cs` | `ExistsById` | 21–30 | `UseReadConnection` |
| `BaseDeletableLongRepository.cs` | `GetById` | 32–41 | `UseReadConnection` |
| `BaseDeletableLongRepository.cs` | `List` | 43–51 | `UseReadConnection` |
| `BaseNonDeletableGuidRepository.cs` | `ExistsById` | 21–29 | `UseReadConnection` |
| `BaseNonDeletableGuidRepository.cs` | `GetById` | 31–39 | `UseReadConnection` |
| `BaseNonDeletableGuidRepository.cs` | `List` | 41–48 | `UseReadConnection` |
| `BaseNonDeletableLongRepository.cs` | `ExistsById` | 21–29 | `UseReadConnection` |
| `BaseNonDeletableLongRepository.cs` | `GetById` | 31–39 | `UseReadConnection` |
| `BaseNonDeletableLongRepository.cs` | `List` | 41–48 | `UseReadConnection` |
| `DatabaseRefreshTokenStorage.cs` | `ValidateAsync` | 27–36 | `UseReadConnection` |

Migration-engine reads (`MigrationEngine.cs:105–125`) go directly through `connection` and always need the primary — the read in question is the "what was the last applied migration number" lookup, which only makes sense against the primary.

## Authentication flow: read/write annotation

This walks each auth-related action end-to-end and marks where the database is touched. The refresh-token storage backend is pluggable (`IRefreshTokenStorage`) — there are two implementations and their DB footprint is very different.

### Storage backends

- **`DatabaseRefreshTokenStorage`** (`src/Storm.Api/Authentications/Refresh/Storage/DatabaseRefreshTokenStorage.cs`) persists refresh tokens to `RefreshTokens`. Supports revocation and rotation enforcement. Every mutating operation is a primary write.
- **`JwtRefreshTokenStorage`** (`src/Storm.Api/Authentications/Refresh/Storage/JwtRefreshTokenStorage.cs`) is stateless — every method returns immediately (`Task.CompletedTask` or `Task.FromResult(true)`). There is no revocation; tokens are valid until their JWT expiry. Zero DB traffic.

### Request authentication (`JwtAuthenticator`)

- `src/Storm.Api/Authentications/Jwts/JwtAuthenticator.cs:12–21`.
- Pure crypto first: `JwtService<TAccount>.TryValidateToken` → `JwtTokenService.TryGetId` (`src/Storm.Api/Authentications/Jwts/JwtTokenService.cs:108–145`). No DB.
- On success, resolves `IGuidRepository<TAccount>.GetById(userId)` — a single `UseReadConnection` read.

### Authenticated action pipeline (`BaseAuthenticatedAction`)

- `src/Storm.Api/CQRS/BaseAuthenticatedAction.cs:31–43`. Sequence: `ValidateParameter` → `PrepareParameter` → `_authenticator.Authenticate()` → `Authorize(parameter, account)` → `Action(parameter, account)`.
- Authentication itself is a read. `Authorize` and `Action` are application-defined and can be anything.

### Login (`BaseLoginAction`)

- `src/Storm.Api/Authentications/Refresh/BaseLoginAction.cs:15–44`.
- `AuthenticateCredentials(parameter)` — application-defined; typically a read (e.g. SELECT account by email + verify password hash with `Pbkdf2Passwords`). May include a write if the application records last-login timestamps.
- Access-token + refresh-token generation — in-memory JWT signing, no DB.
- `storage.StoreAsync(account.Id, jti, expiresAt)` — **write when using `DatabaseRefreshTokenStorage`** (`BaseLoginAction.cs:33`); no-op under `JwtRefreshTokenStorage`.
- Transport emit (cookie or JSON) — HTTP-level only.
- Not wrapped in a transaction.

### Refresh (`BaseRefreshAction`)

- `src/Storm.Api/Authentications/Refresh/BaseRefreshAction.cs:18–81`.
- Transport read + CSRF check (`ReadToken`, `ValidateTransport`) — no DB.
- JWT signature validation (`refreshSvc.TryValidateToken`) — no DB.
- `JtiExtractor.Extract` — no DB.
- `storage.ValidateAsync(inboundJti)` — **read** under DB storage (`UseReadConnection`); no-op under JWT storage. Runs outside the transaction (`BaseRefreshAction.cs:41`).
- `IGuidRepository<TAccount>.GetById(accountId)` — read via `UseReadConnection`.
- `ValidateAccount(account)` — application-defined.
- Transaction block (`BaseRefreshAction.cs:67–80`) wraps `storage.RotateAsync` plus transport emit.
  - Under `DatabaseRefreshTokenStorage`: `DELETE` old jti + `INSERT` new jti — **writes on primary**, inside `WithDatabaseTransaction`.
  - Under `JwtRefreshTokenStorage`: no-op; the transaction opens on the primary connection anyway.

### Logout (`BaseLogoutAction`)

- `src/Storm.Api/Authentications/Refresh/BaseLogoutAction.cs:10–28`.
- Read token from transport, extract `jti` — no DB.
- `storage.RevokeAsync(jti)` — **write** under DB storage (`DELETE` where `Jti == jti`); no-op under JWT storage.
- Clear transport — HTTP-level only.
- Not wrapped in a transaction.

### Password hashing

- `src/Storm.Api/Authentications/Pbkdf2Passwords.cs` is pure in-memory. No DB.

### Constant API key authenticator

- `src/Storm.Api/Authentications/ConstantApiKeys/` — key comparison against configured constants. No DB.

## Behaviour in read-only-primary mode

Because every mutating helper today still resolves to the single `DatabaseService.Connection`, "read-only primary" effectively means the primary is unreachable and no replacement is yet designated. With the framework as-is, that means:

- All `UseReadConnection` calls fail (same physical connection as writes).
- All `UseWriteConnection` and `WithDatabaseTransaction` calls fail.
- Migrations cannot start.

The more useful reading is: **assuming `UseReadConnection` is routed to a replica and only `UseWriteConnection` / `WithDatabaseTransaction` target the primary**, which features would still function? Answering that:

### Fully functional (no writes, no primary dependency)

- JWT signature and lifetime validation (`JwtTokenService.TryGetId`).
- Account resolution from JWT (`JwtAuthenticator.Authenticate` → `IGuidRepository.GetById`).
- All repository reads (`ExistsById`, `GetById`, `List`).
- Refresh-token `ValidateAsync` when using `DatabaseRefreshTokenStorage` (read-only check). Under `JwtRefreshTokenStorage` it is a no-op.
- CSRF validation on the cookie refresh-token transport (HMAC-SHA512, no DB).
- `BaseAuthenticatedAction` pipelines whose `Action` only reads — including application-defined `Authorize` methods that only read.

### Broken (writes to primary)

- Any application action whose body calls `UseWriteConnection` or `WithDatabaseTransaction` via the repositories or directly.
- `BaseLoginAction` **when configured with `DatabaseRefreshTokenStorage`** — `storage.StoreAsync` fails. Under `JwtRefreshTokenStorage` the write disappears and login succeeds.
- `BaseRefreshAction` **when configured with `DatabaseRefreshTokenStorage`** — `storage.RotateAsync` inside the transaction fails. Under `JwtRefreshTokenStorage` the transaction opens on the primary (`WithDatabaseTransaction` always targets the primary), so refresh still fails even though the storage call is a no-op. This one is a structural issue of the current transaction wrapper: the transaction is requested regardless of whether the storage operations inside it do anything. See `BaseRefreshAction.cs:67`.
- `BaseLogoutAction` **when configured with `DatabaseRefreshTokenStorage`** — `storage.RevokeAsync` fails; the client cookie cannot be authoritatively revoked. Under `JwtRefreshTokenStorage` logout "succeeds" from the HTTP perspective (transport cleared) but cannot invalidate any existing refresh JWTs server-side — the tokens remain usable until their natural expiry.
- `AuthenticateCredentials` implementations that update a last-login timestamp, login-attempt counter, lockout state, etc. The credential-read portion would succeed, the counter-write would fail.
- `MigrationEngine.Run` at startup (see Startup section below).

### Summary matrix

| Feature | Primary down, replica up (DB refresh storage) | Primary down, replica up (JWT refresh storage) |
|---|---|---|
| Validate access token | Works | Works |
| Resolve account from token | Works (read on replica) | Works |
| Read-only business endpoints | Work (reads on replica) | Work |
| Write business endpoints | Fail | Fail |
| Login — credential check | Works if read-only | Works if read-only |
| Login — refresh-token persistence | Fails (`StoreAsync` write) | Works (no-op) |
| Refresh access token | Fails (transaction on primary + rotation writes) | Fails (transaction still opens on primary even though `RotateAsync` is a no-op) |
| Logout | Fails (`RevokeAsync` write) | "Succeeds" but cannot revoke server-side — tokens still valid until JWT expiry |
| Startup migrations | Fail (see Startup section) | Fail (same) |
| OrmLite SQL logging | Depends on configured `ILogService` sink; not writing to the app database unless you configured it that way | Same |

## Startup and migrations

Migrations are run from `BaseStartup.Configure` — `src/Storm.Api/Launchers/BaseStartup.cs:159–167`:

```
if (_migrationModules is { Length: > 0 })
{
    ConfiguredTaskAwaitable<bool> migrationTask = MigrationHelper.Run(...).ConfigureAwait(false);
    if (WaitForMigrationsBeforeStarting)
    {
        migrationTask.GetAwaiter().GetResult();
    }
}
```

- `WaitForMigrationsBeforeStarting` defaults to `true` (`BaseStartup.cs:48`). With the default, startup blocks on the migration task synchronously.
- `MigrationHelper.Run` — `src/Storm.Api/Databases/Migrations/MigrationHelper.cs:10–19` — resolves a scoped `IDatabaseService` and calls `MigrationEngine.Run`.
- `MigrationEngine.Run` — `MigrationEngine.cs:23–55` — awaits `databaseService.Connection` (primary), creates the bookkeeping table, opens a transaction on that connection, then loops `operation.Apply(connection)` for each pending migration.

Consequences during a primary outage:
- With `WaitForMigrationsBeforeStarting = true`: `databaseService.Connection` hangs or faults until the primary becomes reachable. Startup is blocked; the HTTP listener never comes up.
- With `WaitForMigrationsBeforeStarting = false`: HTTP comes up, but the background migration task will fault. Endpoints that depend on not-yet-applied schema will fail once the primary recovers and migrations finally run.

Migrations inherently require the primary. There is no design question here — only an operational one: do we want startup to block, retry, or come up serving reads only?

## Structural constraints for a future read-replica design

These are the hard constraints the implementer has to respect. They are stated as observations, not solutions.

1. **`DatabaseService` caches a single connection per scope.** `DatabaseService.cs:22–32`. A split between primary and replica connections needs either two caches or a redesign of how `IDatabaseService` exposes connections.
2. **Transactions bind to a single connection at creation time.** `DatabaseService.cs:34–45`, `DatabaseTransaction.cs:13–19`. A transaction must run on the primary, and any `UseReadConnection` called while a transaction is active on that scope must fall back to the primary connection — otherwise reads inside the transaction see replica state that does not include uncommitted writes on the primary (replication lag on top of that).
3. **`DummyTransaction` is the nesting behaviour.** `DatabaseService.cs:37–40`, `DummyTransaction.cs:5–19`. Any new routing logic must preserve this "caller inside a live transaction just reuses it" contract.
4. **`UseConnection` currently has no declared intent.** `BaseDatabaseService.cs:15–25`. It is unused inside the framework's own code (every caller uses the Read or Write variant) but it is public, so application code may depend on it. The design has to choose semantics for it — the safe default is "treat as write / primary" so existing callers remain correct under an outage.
5. **`BaseRefreshAction` opens a transaction even when the storage backend is stateless.** `BaseRefreshAction.cs:67`. Under `JwtRefreshTokenStorage` the transaction body does no DB work, but `WithDatabaseTransaction` still acquires the primary connection and opens a transaction on it. If the design wants refresh to work on replicas when storage is stateless, this transaction cannot be unconditional.
6. **Migrations are always-primary.** `MigrationEngine.cs:23–55`. The connection acquired here must be the primary; migration code cannot be routed through read/write split logic.
7. **`DisposeConnectionMiddleware` disposes every `IDatabaseService` in the request scope.** `DisposeConnectionMiddleware.cs:11–19`. If the split introduces a second `IDatabaseService` (or second connection inside the same service), disposal has to cover both.
8. **`LogServiceDatabaseLog` does not write to the database itself** (`LogServiceDatabaseLog.cs`), so logging is not in scope for the read-replica design. If an application configures an `ILogService` sink that writes to the same database, that is an app-level concern.
9. **`OrmLiteInterceptors` are in-memory only.** `OrmLiteInterceptors.cs:25–62`. No hidden writes happen via `OrmLiteConfig.InsertFilter` / `UpdateFilter` in framework code.

## Recommendations for "what can we do during an outage?" (framework defaults)

These follow from the findings above; they are defaults to aim for in the design, not implementation steps.

- JWT-stateless refresh storage (`JwtRefreshTokenStorage`) is the only configuration that keeps login functional on replicas, at the cost of no server-side revocation. Logout becomes best-effort (transport-cleared, token-still-valid) until natural expiry.
- Pure-read endpoints continue to work automatically once `UseReadConnection` is routed to a replica, subject to replication lag.
- Writes need to surface a clean failure (e.g. HTTP 503) rather than blocking on a dead primary. The blocking today is whatever timeout the `IDbConnectionFactory.Open` call imposes; that is the place to add a fast-fail path.
- Startup needs a policy choice: block on primary (current default, service is down during outage) vs. boot-without-migrations (service reads work, write endpoints 503 until primary returns). Both are feasible today by flipping `WaitForMigrationsBeforeStarting`.
