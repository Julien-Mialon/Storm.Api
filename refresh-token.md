# Refresh Token Mechanism — Implementation Tasks

## Architecture

3-layer design with orthogonal concerns:

- **Storage** (global): how refresh tokens are persisted/validated server-side
- **Transport** (per-controller): how refresh tokens reach the client
- **Logic**: orchestration (login, refresh, logout)

## Completed Tasks

### Layer 1: Storage

- [x] `IRefreshTokenStorage` interface — `StoreAsync`, `ValidateAsync`, `RevokeAsync`, `RevokeAllForAccountAsync`
- [x] `JwtRefreshTokenStorage` — stateless no-op implementation (no server-side state)
- [x] `RefreshTokenEntity` — OrmLite entity for `RefreshTokens` table (Guid PK, AccountId, Jti, ExpiresAt)
- [x] `RefreshTokenMigration` — `BaseMigration` creating the `RefreshTokens` table
- [x] `DatabaseRefreshTokenStorage` — OrmLite-backed implementation with JTI validation and revocation

### Layer 2: Transport

- [x] `IRefreshTokenTransport` interface — `ReadToken`, `ValidateTransport`, `EmitToken`, `ClearToken`
- [x] `RefreshTokenTransportMode` enum — `Json`, `Cookie`
- [x] `RefreshTokenTransportAttribute` — `ActionFilterAttribute` for per-controller transport selection
- [x] `IRefreshTokenTransportResolver` + `RefreshTokenTransportResolver` — resolves keyed transport from `HttpContext.Items`
- [x] `JsonRefreshTokenTransport` — reads/writes refresh token in request/response body
- [x] `CookieTransportConfiguration` — config POCO with CSRF key, cookie settings, and `IConfiguration` loader
- [x] `CookieRefreshTokenTransport` — HttpOnly cookie transport with HMAC-SHA512 CSRF protection

### Layer 3: Logic

- [x] `BaseLoginAction<TParameter, TAccount>` — authenticate credentials → generate tokens → store → emit
- [x] `BaseRefreshAction<TAccount>` — read → validate transport (CSRF) → validate JWT → validate storage → rotate
- [x] `BaseLogoutAction` — read → revoke storage → clear transport

### Shared

- [x] `LoginResponse` DTO — `AccessToken`, `ExpiresAt`, `RefreshToken?`, `CsrfToken?`
- [x] `RefreshTokenMarker` — DI generic discriminator for refresh JWT config/service
- [x] `RefreshTokenParameter` — input DTO with `RefreshToken` field
- [x] `JtiExtractor` — internal helper to read `jti` claim from JWT without full validation

### Bootstrapper

- [x] `RefreshTokenBootstrapper` — `AddRefreshTokens<TAccount>(config)` entry point
- [x] `RefreshTokenBuilder` — fluent API: `.WithJwtStorage()`, `.WithDatabaseStorage()`, `.WithJsonTransport()`, `.WithCookieTransport(config)`

## Usage Example

```csharp
// In Startup.ConfigureServices:
services.AddRefreshTokens<AccountEntity>(refreshJwtConfig)
    .WithDatabaseStorage()
    .WithJsonTransport()
    .WithCookieTransport(cookieConfig);

// On controllers:
[RefreshTokenTransport(RefreshTokenTransportMode.Json)]
public class ApiAuthController : BaseController { ... }

[RefreshTokenTransport(RefreshTokenTransportMode.Cookie)]
public class WebAuthController : BaseController { ... }
```

## Future Considerations

- Expired token cleanup (SQL job or dedicated service)
- Token deny-list for JWT storage revocation
- Refresh token family detection (reuse detection)
