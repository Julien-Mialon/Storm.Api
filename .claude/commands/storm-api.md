You are helping implement a C# backend using the **Storm.Api** framework. Follow all patterns and constraints below exactly — they are non-negotiable.

---

## Framework Overview

Storm.Api is an opinionated ASP.NET Core framework built on CQRS. Business logic lives in **Action classes**, never in controllers. Controllers are thin, source-generated glue. Three NuGet packages:

- `Storm.Api` — core framework (net10.0)
- `Storm.Api.Dtos` — response types, shared with clients (netstandard2.0)
- `Storm.Api.SourceGenerators` — Roslyn generator for `[WithAction<T>]` (netstandard2.0)

For specific patterns, use the focused commands:
- `/storm-api-init` — new project setup (NuGet packages, Program.cs, Startup.cs, code style)
- `/storm-api-endpoint` — actions, controllers, exceptions, response DTOs
- `/storm-api-auth` — IActionAuthenticator, JWT, API key, custom authenticators, refresh tokens
- `/storm-api-database` — entities, OrmLite queries, repositories
- `/storm-api-database-migrations` — writing and registering migrations
- `/storm-api-email` — email sending with Resend, temporary email detection

---

## Logging

**Never use `ILogger<T>` or `Microsoft.Extensions.Logging`.** Use `ILogService` exclusively.

```csharp
// Resolve inside an action or service
var log = Resolve<ILogService>();

// Log with structured properties
log.Log(LogLevel.Warning, x => x
    .WriteProperty("message", "Something went wrong")
    .WriteProperty("userId", userId)
    .WriteException(ex));

// Convenience extension methods
log.Information(x => x.WriteProperty("event", "user_created").WriteProperty("id", id));
log.Warning(x => x.WriteProperty("message", "Rate limit hit"));
log.Error(x => x.WriteProperty("message", "Unexpected failure").WriteException(ex));
```

Register in `Startup.ConfigureServices`:
```csharp
RegisterConsoleLogger(services, LogLevel.Debug);
// or
RegisterSerilogLogger(services, "Serilog");
// or
RegisterElasticSearchLogger(services, "ElasticSearch");
// or (background queue)
RegisterElasticSearchHostedLogger(services, "ElasticSearch");
```

---

## Key Extension Methods

```csharp
// Instead of Task.FromResult()
return myValue.AsTask();
return null.AsTaskNullable<int>();

// String helpers
str.IsNullOrEmpty()             // bool
str.IsNotNullOrEmpty()          // bool
str.ValueIfNullOrEmpty("fallback")
str.NullIfEmpty()               // returns null if empty
str.OrEmpty()                   // coalesce to ""

// Execute action outside controller context
await services.ExecuteAction<MyAction, MyParam, MyOutput>(param);
await services.ExecuteWithScope(async sp => { ... });

// Service resolution inside actions/services
var svc = Resolve<IMyService>();
var keyed = ResolveKeyed<IMyService>("key");
```

---

## Anti-Patterns to Avoid

| ❌ Wrong | ✅ Correct |
|---|---|
| `Task.FromResult(value)` | `value.AsTask()` |
| Constructor-inject services | `Resolve<T>()` inside methods |
| `ILogger<T>` | `ILogService` |
| Entity Framework / Dapper | ServiceStack.OrmLite |
| Logic in controller | Logic in Action class |
| `DateTime.Now` / `DateTime.UtcNow` / `DateTimeOffset.UtcNow` | `Resolve<TimeProvider>().GetUtcNow().UtcDateTime` (or inject `TimeProvider` via constructor in non-`Resolve` classes) |