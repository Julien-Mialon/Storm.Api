You are helping implement authentication using the **Storm.Api** framework. Follow all patterns below exactly. For global rules (logging, extensions, anti-patterns), see `/storm-api`.

The user's request: $ARGUMENTS

---

## How Authentication Works

`BaseAuthenticatedAction` resolves the current account by calling `IActionAuthenticator<TAccount>.Authenticate()` before invoking `Action()`. You must register an implementation of this interface in DI for every account type you use.

The framework provides two built-in strategies (JWT and constant API key) plus a base class for custom token-based authenticators.

---

## Option 1 — JWT Authentication

The account entity **must implement `IGuidEntity`** (i.e. extend `BaseGuidEntity` or `BaseDeletableGuidEntity`). The JWT authenticator validates the token, extracts the user's Guid, and loads the entity from the database via `IGuidRepository<TAccount>`.

### appsettings.json

```json
{
  "Jwt": {
    "Key": "<base64-encoded-secret>",
    "Issuer": "my-app",
    "Audience": "my-app",
    "Duration": 3600
  }
}
```

### Register in Startup.ConfigureServices

```csharp
services.AddJwtAuthenticator<CurrentUser>(
    configuration.GetSection("Jwt").LoadJwtConfiguration<CurrentUser>()
);
// Optional overrides (defaults shown):
// headerName: "Authorization", queryParameterName: "Authorization", tokenType: "Bearer"
```

`AddJwtAuthenticator` also registers `IGuidRepository<CurrentUser>` automatically. Make sure `CurrentUser` has a matching repository registered (or let the JWT registration handle it).

### Generate a token (e.g. in a login action)

```csharp
var jwtService = Resolve<JwtService<CurrentUser>>();
(string token, TimeSpan duration) = jwtService.GenerateToken(user.Id);
```

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

Extend `BaseTokenAuthenticator<TAccount>` to extract a token from a header or query parameter and resolve the account yourself:

```csharp
public class MyAuthenticator(IServiceProvider services)
    : BaseTokenAuthenticator<CurrentUser>(
        services,
        headerName: "Authorization",
        queryParameterName: null,
        tokenType: "Bearer")
{
    protected override async Task<CurrentUser?> Authenticate(string token)
    {
        // token is already stripped of the "Bearer " prefix
        var repo = Resolve<UserRepository>();
        return await repo.GetByToken(token);
    }
}

// Register in ConfigureServices:
services.AddScoped<IActionAuthenticator<CurrentUser>, MyAuthenticator>();
```

For cases where you don't need header/query extraction, implement `IActionAuthenticator<TAccount>` directly:

```csharp
public class MyAuthenticator(IServiceProvider services) : BaseServiceContainer(services), IActionAuthenticator<CurrentUser>
{
    public async Task<CurrentUser?> Authenticate()
    {
        var context = Resolve<IHttpContextAccessor>();
        // ... your logic
    }
}
```

---

## Using Authentication in Actions

```csharp
public class GetProfileQuery(IServiceProvider services)
    : BaseAuthenticatedAction<GetProfileParameter, ProfileDto, CurrentUser>(services)
{
    // Optional: add authorization checks before Action() is called
    protected override async Task Authorize(GetProfileParameter parameter, CurrentUser account)
    {
        if (!account.IsActive)
            throw new DomainHttpCodeException(HttpStatusCode.Forbidden);
    }

    protected override async Task<ProfileDto> Action(GetProfileParameter parameter, CurrentUser account)
    {
        // account is guaranteed non-null here
        return new ProfileDto { Id = account.Id, Name = account.Name };
    }
}
```

**Pipeline:** `ValidateParameter()` → `PrepareParameter()` → `Authenticate()` → `Authorize()` → `Action(parameter, account)`

If `Authenticate()` returns `null`, the action throws HTTP 401 automatically — no need to null-check the account.

---

## Refresh Tokens

Storm.Api provides a complete refresh token system with pluggable **storage** (where tokens are persisted) and **transport** (how tokens are sent to/from clients).

### appsettings.json

```json
{
  "RefreshToken": {
    "Key": "<base64-encoded-secret>",
    "Issuer": "my-app",
    "Audience": "my-app",
    "Duration": 604800
  },
  "CookieTransport": {
    "CookieName": "refresh_token",
    "CookiePath": "/auth/refresh",
    "Secure": true,
    "HttpOnly": true,
    "SameSite": "Strict",
    "Domain": null,
    "CsrfKey": "<base64-encoded-key-required-when-SameSite-is-not-Strict>",
    "CsrfHeaderName": "X-CSRF-Token"
  }
}
```

### Register in Startup.ConfigureServices

```csharp
// Fluent builder — pick one storage + one or more transports
services.AddRefreshTokens<CurrentUser>(
    configuration.GetSection("RefreshToken").LoadJwtConfiguration<RefreshTokenMarker>()
)
.WithDatabaseStorage()       // JTI stored in DB, supports revocation
// or: .WithJwtStorage()     // stateless, no revocation support
.WithJsonTransport()         // refresh token in JSON response body
.WithCookieTransport(        // refresh token in HttpOnly cookie + CSRF
    configuration.GetSection("CookieTransport").LoadCookieTransportConfiguration()
);
```

When using database storage, register the migration module in the Startup constructor:

```csharp
UseMigrationModules(new RefreshTokenMigrationModule(), new AppMigrationModule());
```

### Transport selection via attribute

Apply `[RefreshTokenTransport]` on the controller or action method to select which transport is used:

```csharp
[RefreshTokenTransport(RefreshTokenTransportMode.Json)]
public partial class MobileAuthController(IServiceProvider services) : BaseController(services) { ... }

[RefreshTokenTransport(RefreshTokenTransportMode.Cookie)]
public partial class WebAuthController(IServiceProvider services) : BaseController(services) { ... }
```

### Login action

Extend `BaseLoginAction<TParameter, TAccount>` — implement `AuthenticateCredentials` to validate the user:

```csharp
public class LoginCommand(IServiceProvider services)
    : BaseLoginAction<LoginParameter, CurrentUser>(services)
{
    protected override async Task<CurrentUser?> AuthenticateCredentials(LoginParameter parameter)
    {
        var repo = Resolve<UserRepository>();
        var user = await repo.GetByEmail(parameter.Email);
        if (user is null || !VerifyPassword(parameter.Password, user.PasswordHash))
            return null;
        return user;
    }
}
```

Returns `LoginResponse` with `AccessToken`, `ExpiresAt`, and optionally `RefreshToken` (JSON transport) or `CsrfToken` (cookie transport).

### Refresh action

Extend `BaseRefreshAction<TAccount>` — implement `ValidateAccount` to check the account is still valid:

```csharp
public class RefreshCommand(IServiceProvider services)
    : BaseRefreshAction<CurrentUser>(services)
{
    protected override async Task ValidateAccount(CurrentUser account)
    {
        if (!account.IsActive)
            throw new DomainHttpCodeException(HttpStatusCode.Forbidden);
    }
}
```

The parameter type is `RefreshTokenParameter` (has a `RefreshToken` property used by JSON transport; cookie transport reads from the cookie instead).

### Logout action

`BaseLogoutAction` is a concrete action — use it directly, no subclass needed:

```csharp
// Controller
[HttpPost("logout")]
[WithAction<BaseLogoutAction>]
public partial Task<ActionResult<Response>> Logout([FromBody] RefreshTokenParameter body);
```

### Controller example

```csharp
[RefreshTokenTransport(RefreshTokenTransportMode.Json)]
public partial class AuthController(IServiceProvider services) : BaseController(services)
{
    [HttpPost("login")]
    [WithAction<LoginCommand>]
    public partial Task<ActionResult<Response<LoginResponse>>> Login([FromBody] LoginParameter body);

    [HttpPost("refresh")]
    [WithAction<RefreshCommand>]
    public partial Task<ActionResult<Response<LoginResponse>>> Refresh([FromBody] RefreshTokenParameter body);

    [HttpPost("logout")]
    [WithAction<BaseLogoutAction>]
    public partial Task<ActionResult<Response>> Logout([FromBody] RefreshTokenParameter body);
}
```

### Storage options

| Storage | Registration | Use when |
|---------|-------------|----------|
| Database | `.WithDatabaseStorage()` | You need token revocation, logout, or "revoke all sessions" |
| JWT | `.WithJwtStorage()` | Stateless is acceptable, no revocation needed |

### Transport options

| Transport | Registration | Client sends refresh token via | Response includes |
|-----------|-------------|-------------------------------|-------------------|
| JSON | `.WithJsonTransport()` | `RefreshToken` in request body | `refresh_token` in JSON body |
| Cookie | `.WithCookieTransport(config)` | HttpOnly cookie (automatic) | `csrf_token` in JSON body |

---

## Anti-Patterns to Avoid

| ❌ Wrong | ✅ Correct |
|---|---|
| `services.AddScoped<IActionAuthenticator<T>, MyAuth>()` manually for JWT | `services.AddJwtAuthenticator<T>(config)` |
| Null-check `account` inside `Action()` | Framework guarantees non-null; throw in `Authorize()` if needed |
| Read auth token manually in an action | Implement `IActionAuthenticator<TAccount>` and let the framework call it |
