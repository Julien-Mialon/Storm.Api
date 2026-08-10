# Storm.Api
Framework to build APIs

## Changelog

### 10.0.16 / 10.0.17 (August 10th)

#### Fixes
- Fixed disposal order of JsonLogWriter which could cause an unflushed stream to be disposed before flush.

### 10.0.15 (June 5th)

#### Improvements
- **Source generator overhaul** — Rewrote the `[WithAction<T>]` generator to use `ForAttributeWithMetadataName` with a fully cached pipeline (caching context, diagnostic caching) for significantly faster, more incremental builds. Refactored the `ActionMethod` generator for efficiency and added a test suite.
- **GUID logging** — `JsonLogWriter` now supports writing `Guid` values directly through `IArrayWriter`/`IObjectWriter`.

#### Fixes
- Fixed attribute generation code in the source generator.

### 10.0.13 (May 21st)

#### Fixes
- Fixed `DefaultValuesExtensions` concurrency issue.
- Fixed `IsSuccess` not being set to `true` for `PaginatedResponse`.

### 10.0.11 (May 13th)

#### New Features
- **Default value from `Type`** — Added a `DefaultValuesExtensions` overload to generate a default value from a `Type` instance.

### 10.0.10 (May 9th)

#### Improvements
- **OpenAPI documentation** — The source generator now supports specifying multiple attributes on actions for OpenAPI documentation.
- **Customizable log timestamp field** — The `TimestampLogAppender` field name is now configurable.

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