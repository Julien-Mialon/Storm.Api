# Storm.Api
Framework to build APIs

## Changelog

### 10.0.9 (April 13th)

- Fix function **WithDatabaseTransaction** to pass the correct connection in the callback

### 10.0.8 (April 12th)

- Fix **BaseRefreshAction** / **BaseLogoutAction** to have generic input parameters

### 10.0.7 (April 11th)

- **IRepository.ExistsById** - New function added to check for existence

### 10.0.6 (April 11th)

#### New Features
- **Refresh token authentication** — New `BaseLoginAction`, `BaseRefreshAction`, and `BaseLogoutAction` with pluggable storage (database or JWT-only) and transport (cookie or JSON body). Replaces the previous cookie-only `JwtRefreshCookieService`.
- **Sequential GUID generation** — `SequentialGuid.NewGuid()` produces database-friendly ordered GUIDs: Guid v7 for PostgreSQL/MySQL/SQLite, COMB GUIDs for SQL Server.

#### Improvements
- **TimeProvider adoption** — All date/time usage now goes through the `TimeProvider` abstraction instead of `DateTime.Now`/`DateTime.UtcNow`, enabling deterministic testing.
- **Code quality pass** — Enforced `.editorconfig` rules, resolved all warnings under `TreatWarningsAsErrors`, and cleaned up code style across the solution.