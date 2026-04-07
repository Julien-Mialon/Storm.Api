---
name: storm-api-endpoint
description: Implement a C# endpoint using the Storm.Api framework with CQRS actions, controllers, source generators, exceptions, and response DTOs.
user-invocable: true
disable-model-invocation: false
---

You are helping implement a C# endpoint using the **Storm.Api** framework. Follow all patterns below exactly. For global rules (logging, extensions, anti-patterns), see `/storm-api`.

The user's request: $ARGUMENTS

---

## Actions (Core CQRS Pattern)

All business logic lives in Action classes. **Never put logic in controllers.**

### Basic Action

```csharp
public class MyQueryParameter
{
    public required string Id { get; init; }
}

public class MyQuery(IServiceProvider services) : BaseAction<MyQueryParameter, MyDto>(services)
{
    protected override async Task<MyDto> Action(MyQueryParameter parameter)
    {
        return await UseReadConnection(db => db.SingleByIdAsync<MyEntity>(parameter.Id));
    }
}
```

**Rules:**
- Always use primary constructor `(IServiceProvider services)` — never inject individual services
- `BaseAction<TParameter, TOutput>` — one abstract method to implement: `Action(parameter)`
- Execution pipeline: `ValidateParameter()` → `PrepareParameter()` → `Action(parameter)`
- Override `ValidateParameter` to return `false` to throw HTTP 400
- Override `PrepareParameter` to transform/enrich the parameter before `Action` is called
- Use `Unit` as output when there is no response body: `BaseAction<MyParam, Unit>`, return `Unit.Default`

### Authenticated Action

```csharp
public class SecureCommand(IServiceProvider services)
    : BaseAuthenticatedAction<SecureCommandParameter, Unit, CurrentUser>(services)
{
    protected override async Task Authorize(SecureCommandParameter parameter, CurrentUser account)
    {
        if (!account.IsAdmin)
            throw new DomainHttpCodeException(HttpStatusCode.Forbidden);
    }

    protected override async Task<Unit> Action(SecureCommandParameter parameter, CurrentUser account)
    {
        return Unit.Default;
    }
}
```

**Rules:**
- `BaseAuthenticatedAction<TParameter, TOutput, TAccount>` — action receives the resolved account
- Pipeline: `ValidateParameter()` → `PrepareParameter()` → authenticate via `IActionAuthenticator<TAccount>` → `Authorize()` → `Action(parameter, account)`
- Register `IActionAuthenticator<TAccount>` in DI for each account type
- Use `.UnauthorizedIfNull()` on the authenticator result for a clean null check

---

## Controllers & Source Generator

Controllers are `partial` classes. The `[WithAction<T>]` attribute causes the Roslyn generator to implement the partial method automatically.

```csharp
public partial class UsersController(IServiceProvider services) : BaseController(services)
{
    [HttpGet("{id}")]
    [WithAction<GetUserQuery>]
    public partial Task<ActionResult<Response<UserDto>>> GetUser([FromRoute] string id);

    [HttpPost]
    [WithAction<CreateUserCommand>]
    public partial Task<ActionResult<Response>> CreateUser([FromBody] CreateUserRequest body);

    [HttpGet("{id}/avatar")]
    [WithAction<GetAvatarQuery>]
    public partial Task<IActionResult> GetAvatar([FromRoute] string id);
}
```

**Rules:**
- Controller must be `partial`, extend `BaseController`, receive only `IServiceProvider`
- Always use `[WithAction<TAction>]` — never implement the partial method manually
- Generator maps HTTP parameters to the action's parameter type **by property name** (case-insensitive)
- Use `[MapTo(nameof(MyParam.SomeProperty))]` when the HTTP parameter name differs from the parameter class property
- Return types:
  - `Task<ActionResult<Response<T>>>` — typed output
  - `Task<ActionResult<Response>>` — `Unit` output (no body)
  - `Task<IActionResult>` — file download (`ApiFileResult` output)

---

## Exception Handling

```csharp
// Business error → HTTP 200, IsSuccess=false
throw new DomainException("ERROR_CODE", "Human readable message");

// HTTP error → specific status code
throw new DomainHttpCodeException(HttpStatusCode.NotFound, "NOT_FOUND", "Resource not found");
throw new DomainHttpCodeException(HttpStatusCode.Forbidden);

// Null-check shortcuts (prefer these over manual throws)
var entity = await GetEntity(id).NotFoundIfNull("NOT_FOUND", "Entity not found");
var user   = await Authenticate().UnauthorizedIfNull();
var item   = GetItem(id).BadRequestIfNull("MISSING", "Item required");
var data   = Load(id).ForbiddenIfNull();
```

**Rules:**
- `DomainException` → caller gets HTTP 200 with `{ "is_success": false, "error_code": "...", "error_message": "..." }`
- `DomainHttpCodeException` → caller gets the specified HTTP status code
- Extension helpers work on `T?` and `Task<T?>` — use them everywhere instead of `if (x == null) throw`
- Never throw raw exceptions for expected error conditions

---

## Response DTOs

All endpoints return framework response wrappers. Never return raw objects.

Types (from `Storm.Api.Dtos`):
- `Response` — no data (`{ "is_success": true }`)
- `Response<T>` — with typed data payload (`{ "is_success": true, "data": { ... } }`)
- `PaginatedResponse<T>` — for paginated lists (has `Page`, `Count`, `TotalCount`)
- `ApiFileResult` — for file downloads: `ApiFileResult.Create(bytes, "application/pdf", "file.pdf")`

For a full paginated response example (action + controller), see [examples/paginated-response.md](examples/paginated-response.md).

---

## Anti-Patterns to Avoid

| ❌ Wrong | ✅ Correct |
|---|---|
| Logic in controller | Logic in Action class |
| Implement partial method manually | Let `[WithAction<T>]` generate it |
| `if (x == null) throw new ...` | `.NotFoundIfNull()` / `.UnauthorizedIfNull()` etc. |
| `if (x == false) throw new ...` | `.NotFoundIfFalse()` / `.UnauthorizedIfFalse()` etc. |
