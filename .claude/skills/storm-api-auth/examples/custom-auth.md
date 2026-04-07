# Custom Token Authenticator

## Extending BaseTokenAuthenticator

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

## Implementing IActionAuthenticator directly

For cases where you don't need header/query extraction:

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
