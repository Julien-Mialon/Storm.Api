# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

Storm.Api is a C# framework (published to NuGet) for building ASP.NET Core APIs using a CQRS pattern with Roslyn source generators. It ships three packages: `Storm.Api`, `Storm.Api.Dtos`, and `Storm.Api.SourceGenerators`.

## Commands

```bash
# Build
dotnet build Storm.Api.slnx

# Build release (sets version)
dotnet build -c Release /property:Version=10.0.5

# Run sample project
dotnet run --project sample/Storm.Api.Sample/Storm.Api.Sample.csproj

# Release (builds + pushes to NuGet; requires NUGET_API_KEY env var)
./release.sh
```

There are no test projects. The sample project serves as the integration reference.

## Architecture

### Core Pattern: CQRS via Actions

Business logic lives in **Action classes**, not controllers. An action is a class extending `BaseAction<TParameter, TOutput>` with one abstract method to implement:

```csharp
public class HelloQuery(IServiceProvider services) : BaseAction<HelloQueryParameter, string>(services)
{
    protected override Task<string> Action(HelloQueryParameter parameter) { ... }
}
```

`BaseAction.Execute()` calls three virtual hooks in order: `ValidateParameter()` → `PrepareParameter()` → `Action()`. Override the first two to add validation/transformation without touching the core logic.

For authenticated actions, extend `BaseAuthenticatedAction<TParameter, TOutput, TAccount>` — the account is resolved and passed into `Action(parameter, account)`.

### Source Generator: `[WithAction<T>]`

Controllers use `partial` methods decorated with `[WithAction<TAction>]`. The source generator (`Storm.Api.SourceGenerators`) implements these partial methods at compile time, wiring up parameter mapping and action execution automatically:

```csharp
public partial class MyController(IServiceProvider services) : BaseController(services)
{
    [HttpGet("/hello/{name}")]
    [WithAction<HelloQuery>]
    public partial Task<ActionResult<Response<string>>> HelloWorld([FromRoute] string name);
}
```

The generator maps HTTP parameters to the action's parameter type by property name. Use `[MapTo(nameof(Param.Property))]` when names differ.

### BaseController

Controllers extend `BaseController` and receive `IServiceProvider` (not individual services). Actions are invoked via:
- `Action<TAction, TParameter>()` — for `Unit` output (no response body)
- `Action<TAction, TParameter, TOutput>()` — for typed response
- `FileAction<TAction, TParameter>()` — for `ApiFileResult`

All wrap results in `Response` / `Response<T>` with `IsSuccess`, `ErrorCode`, `ErrorMessage`.

### Exception Handling

- `DomainException` — business-level error; returns HTTP 200 with `IsSuccess = false`
- `DomainHttpCodeException` — maps to a specific HTTP status code with optional `ErrorCode`/`ErrorMessage`

### Database Layer

`BaseDatabaseService` (base of `BaseAction`) uses **ServiceStack.OrmLite**. Database connections, migrations, and bootstrapping are in `src/Storm.Api/Databases/`. SQL migrations extend `BaseStartup` via `UseMigrationModules()`.

### Application Startup

Apps extend `BaseStartup` and `DefaultLauncher<TStartup>`. `BaseStartup` configures compression, CORS, OpenAPI, and optionally runs migrations before accepting traffic (`WaitForMigrationsBeforeStarting`).

### Key Namespaces

| Namespace | Purpose |
|-----------|---------|
| `Storm.Api.CQRS` | `BaseAction`, `IAction`, `Unit`, exceptions |
| `Storm.Api.Controllers` | `BaseController` |
| `Storm.Api.Databases` | OrmLite wrappers, migrations, repositories |
| `Storm.Api.Launchers` | `BaseStartup`, `DefaultLauncher` |
| `Storm.Api.Logs` | `ILogService`, sinks (Console, Serilog, Elasticsearch) |
| `Storm.Api.Extensions` | 60+ extension methods (`AsTask()`, `ValueIfNullOrEmpty()`, etc.) |
| `Storm.Api.SourceGenerators` | Roslyn generator for `[WithAction<T>]` |
| `Storm.Api.Dtos` | `Response<T>`, `ApiFileResult`, pagination — targets .NET Standard 2.0 |

## Project Constraints

- `TreatWarningsAsErrors=true` and `EnforceCodeStyleInBuild=true` are set globally — all warnings must be resolved.
- `Storm.Api.Dtos` and `Storm.Api.SourceGenerators` target .NET Standard 2.0 for broad compatibility; keep them free of .NET-only APIs.
