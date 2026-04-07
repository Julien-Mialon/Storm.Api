# JWT Authentication Setup

## appsettings.json

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

## Register in Startup.ConfigureServices

```csharp
services.AddJwtAuthenticator<CurrentUser>(
    configuration.GetSection("Jwt").LoadJwtConfiguration<CurrentUser>()
);
// Optional overrides (defaults shown):
// headerName: "Authorization", queryParameterName: "Authorization", tokenType: "Bearer"
```

`AddJwtAuthenticator` also registers `IGuidRepository<CurrentUser>` automatically. Make sure `CurrentUser` has a matching repository registered (or let the JWT registration handle it).

## Generate a token (e.g. in a login action)

```csharp
var jwtService = Resolve<JwtService<CurrentUser>>();
(string token, TimeSpan duration) = jwtService.GenerateToken(user.Id);
```
