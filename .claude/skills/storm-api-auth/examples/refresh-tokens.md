# Refresh Tokens

## appsettings.json

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

## Register in Startup.ConfigureServices

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

## Transport selection via attribute

Apply `[RefreshTokenTransport]` on the controller or action method to select which transport is used:

```csharp
[RefreshTokenTransport(RefreshTokenTransportMode.Json)]
public partial class MobileAuthController(IServiceProvider services) : BaseController(services) { ... }

[RefreshTokenTransport(RefreshTokenTransportMode.Cookie)]
public partial class WebAuthController(IServiceProvider services) : BaseController(services) { ... }
```

## Login action

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

## Refresh action

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

## Logout action

`BaseLogoutAction` is a concrete action — use it directly, no subclass needed:

```csharp
[HttpPost("logout")]
[WithAction<BaseLogoutAction>]
public partial Task<ActionResult<Response>> Logout([FromBody] RefreshTokenParameter body);
```

## Full controller example

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
