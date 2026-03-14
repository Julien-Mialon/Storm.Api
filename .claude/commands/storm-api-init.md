You are helping initialize a new **Storm.Api** project. Follow all patterns below exactly — this covers the one-time project setup.

---

## NuGet Packages

```xml
<PackageReference Include="Storm.Api" Version="*" />
<PackageReference Include="Storm.Api.Dtos" Version="*" />
<PackageReference Include="Storm.Api.SourceGenerators" Version="*" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />

<!-- Pick one OrmLite dialect -->
<PackageReference Include="ServiceStack.OrmLite.SqlServer" Version="*" />
<!-- ServiceStack.OrmLite.PostgreSQL -->
<!-- ServiceStack.OrmLite.MySql -->
<!-- ServiceStack.OrmLite.Sqlite -->
```

**Do NOT add:** Entity Framework Core, Dapper, `Microsoft.Extensions.Logging.Abstractions`, any other ORM.

---

## Program.cs

```csharp
DefaultLauncher<Startup>.RunWebHost(args);
```

With options:
```csharp
DefaultLauncher<Startup>.RunWebHost(args, options =>
{
    options.UseNewtonsoftJson = true; // only if Newtonsoft is explicitly required
    options.UseVault = true;          // load vault.local.json + Hashicorp Vault
    options.SetDatabaseDebug = true;  // verbose OrmLite SQL logging
});
```

Always prefer `System.Text.Json` — only set `UseNewtonsoftJson` when explicitly instructed.

---

## Startup.cs

```csharp
public class Startup(IConfiguration configuration, IWebHostEnvironment env)
    : BaseStartup(configuration, env)
{
    public Startup(...) : base(...)
    {
        // Register migration modules here (not in ConfigureServices)
        UseMigrationModules(new AppMigrationModule());

        // Block HTTP traffic until migrations finish (optional)
        // WaitForMigrationsBeforeStarting = true;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        // Logger — pick one
        RegisterConsoleLogger(services, LogLevel.Debug);
        // RegisterSerilogLogger(services, "Serilog");
        // RegisterElasticSearchLogger(services, "ElasticSearch");
        // RegisterElasticSearchHostedLogger(services, "ElasticSearch"); // background queue

        // Repositories
        services.AddRepository<UserEntity, UserRepository>();         // Guid PK
        services.AddLongRepository<ProductEntity, ProductRepository>(); // long PK

        // Authentication
        services.AddScoped<IActionAuthenticator<CurrentUser>, JwtAuthenticator>();

        // Other scoped services
        services.AddScoped<MyService>();
    }
}
```

`BaseStartup` automatically configures: CORS (all origins), OpenAPI/Scalar (dev only), Brotli+Gzip compression, request localization, database module.

---

## Code Style (Enforced Globally)

Set these in your `.csproj` or `Directory.Build.props`:

```xml
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
<LangVersion>latest</LangVersion>
<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
```