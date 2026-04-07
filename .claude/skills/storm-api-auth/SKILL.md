---
name: storm-api-auth
description: Implement authentication using the Storm.Api framework with JWT, API key, custom authenticators, and refresh tokens. Use when adding auth to endpoints.
user-invocable: true
disable-model-invocation: false
---

You are helping implement authentication using the **Storm.Api** framework. Follow all patterns below exactly. For global rules (logging, extensions, anti-patterns), see `/storm-api`.

The user's request: $ARGUMENTS

---

## How Authentication Works

`BaseAuthenticatedAction` resolves the current account by calling `IActionAuthenticator<TAccount>.Authenticate()` before invoking `Action()`. You must register an implementation of this interface in DI for every account type you use.

The framework provides two built-in strategies (JWT and constant API key) plus a base class for custom token-based authenticators.

**Pipeline:** `ValidateParameter()` → `PrepareParameter()` → `Authenticate()` → `Authorize()` → `Action(parameter, account)`

If `Authenticate()` returns `null`, the action throws HTTP 401 automatically — no need to null-check the account.

---

## Option 1 — JWT Authentication

The account entity **must implement `IGuidEntity`** (i.e. extend `BaseGuidEntity` or `BaseDeletableGuidEntity`). The JWT authenticator validates the token, extracts the user's Guid, and loads the entity from the database via `IGuidRepository<TAccount>`.

Register with `AddJwtAuthenticator<T>()` — it also registers `IGuidRepository<T>` automatically.

For full JWT setup, config, and token generation examples, see [examples/jwt-setup.md](examples/jwt-setup.md).

### Raw JWT (Guid only, no entity lookup)

When you only need the user's Guid from the token without loading a database entity:

```csharp
services.AddRawJwtAuthenticator(
    configuration.GetSection("Jwt").LoadJwtConfiguration<Guid>()
);
// Action type: BaseAuthenticatedAction<TParam, TOutput, Guid?>
```

---

## Option 2 — Constant API Key

For simple service-to-service authentication where a single shared key is sufficient. The account type must have a parameterless constructor — it is instantiated with `new TAccount()` on a valid key.

```csharp
services.AddConstantApiKeyAuthenticator<ServiceAccount>(
    apiKey: configuration["ApiKey"]!
);
// Optional overrides (defaults shown):
// headerName: "X-ApiKey", queryParameterName: "ApiKey"
```

---

## Option 3 — Custom Token Authenticator

Extend `BaseTokenAuthenticator<TAccount>` to extract a token from a header or query parameter and resolve the account yourself. For cases where you don't need header/query extraction, implement `IActionAuthenticator<TAccount>` directly.

For custom authenticator examples, see [examples/custom-auth.md](examples/custom-auth.md).

---

## Using Authentication in Actions

```csharp
public class GetProfileQuery(IServiceProvider services)
    : BaseAuthenticatedAction<GetProfileParameter, ProfileDto, CurrentUser>(services)
{
    protected override async Task Authorize(GetProfileParameter parameter, CurrentUser account)
    {
        if (!account.IsActive)
            throw new DomainHttpCodeException(HttpStatusCode.Forbidden);
    }

    protected override async Task<ProfileDto> Action(GetProfileParameter parameter, CurrentUser account)
    {
        return new ProfileDto { Id = account.Id, Name = account.Name };
    }
}
```

---

## Refresh Tokens

Storm.Api provides a complete refresh token system with pluggable **storage** (where tokens are persisted) and **transport** (how tokens are sent to/from clients).

| Storage | Registration | Use when |
|---------|-------------|----------|
| Database | `.WithDatabaseStorage()` | You need token revocation, logout, or "revoke all sessions" |
| JWT | `.WithJwtStorage()` | Stateless is acceptable, no revocation needed |

| Transport | Registration | Client sends via | Response includes |
|-----------|-------------|-----------------|-------------------|
| JSON | `.WithJsonTransport()` | `RefreshToken` in request body | `refresh_token` in JSON body |
| Cookie | `.WithCookieTransport(config)` | HttpOnly cookie (automatic) | `csrf_token` in JSON body |

For full refresh token setup (config, registration, login/refresh/logout actions, controller), see [examples/refresh-tokens.md](examples/refresh-tokens.md).

---

## Anti-Patterns to Avoid

| ❌ Wrong | ✅ Correct |
|---|---|
| `services.AddScoped<IActionAuthenticator<T>, MyAuth>()` manually for JWT | `services.AddJwtAuthenticator<T>(config)` |
| Null-check `account` inside `Action()` | Framework guarantees non-null; throw in `Authorize()` if needed |
| Read auth token manually in an action | Implement `IActionAuthenticator<TAccount>` and let the framework call it |
