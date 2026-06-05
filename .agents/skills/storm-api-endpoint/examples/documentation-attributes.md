# OpenAPI Documentation Attributes

The `[WithAction<T>]` source generator scans the action class for documentation attributes and emits matching `[EndpointSummary]`, `[EndpointDescription]`, `[ProducesResponseType<T>]`, and `[OpenApiErrorCodes(...)]` on the generated controller method. Document the contract on the **action**, keep the controller clean.

All attributes are in `Storm.Api.SourceGenerators.ActionMethods`.

## Attribute reference

| Attribute | Multiple | Purpose |
|---|---|---|
| `[Summary("text")]` | no | One-line endpoint summary → `[EndpointSummary]` |
| `[Description("text")]` | yes | Long description (multiple are concatenated with blank lines) → `[EndpointDescription]` |
| `[ErrorCode("CODE", Description = "...")]` | yes | Business error code returned via `DomainException` — listed in the endpoint description and aggregated into the `x-error-codes` OpenAPI extension |
| `[HttpError(HttpStatusCode.X, Description = "...")]` | yes | HTTP error returned via `DomainHttpCodeException` — emits `[ProducesResponseType<Response>(code, "application/problem+json")]` |
| `[SuccessCode(code, Description = "...")]` | yes | Override the default `200` success response (omit for the default) |
| `[MediaType("image/png")]` | no | Override the success media type — required on actions returning `ApiFileResult` |
| `[InternalActionCall<TAction>]` | yes | This action internally calls `TAction` — its `[ErrorCode]` and `[HttpError]` attributes are merged into this action's documentation. Use when delegating via `services.ExecuteAction<>()` or composition |

## Generation behavior

- `[Summary]` text becomes `[EndpointSummary]` on the controller method.
- `[Description]` blocks are joined; if any `[ErrorCode]` is present, the generator prepends an "Error Codes:" list to the description.
- Each `[HttpError(code)]` becomes `[ProducesResponseType<Response>(code, "application/problem+json", Description = ...)]`.
- Without `[SuccessCode]`, the generator emits `[ProducesResponseType<TResponse>(200, "application/json")]` (or the `MediaType` value for file actions).
- All `[ErrorCode]` values across the action and its `[InternalActionCall<>]` chain are de-duplicated and emitted as `[OpenApiErrorCodes("CODE1", "CODE2", ...)]`, which the OpenAPI transformer surfaces as `x-error-codes` on the operation.
- `[InternalActionCall<TAction>]` only pulls `[ErrorCode]` and `[HttpError]` from `TAction` — `[Summary]` / `[Description]` are not inherited.

## Conventions

- Document the contract on the **action**, never on the controller — the controller stays free of OpenAPI noise.
- For shared codes, use `const string` fields so the same identifier appears in `throw new DomainException(...)` and `[ErrorCode(...)]`:
  ```csharp
  [ErrorCode(UserErrors.EMAIL_TAKEN, Description = "Email is already registered")]
  ```
- Pair every `throw new DomainException("CODE", ...)` with a matching `[ErrorCode("CODE", ...)]`.
- Pair every `throw new DomainHttpCodeException(HttpStatusCode.X, ...)` with a matching `[HttpError(HttpStatusCode.X, ...)]`.
- When delegating to another action, use `[InternalActionCall<OtherAction>]` instead of restating its `[ErrorCode]` / `[HttpError]` list.

## Full example

```csharp
using System.Net;
using Storm.Api.CQRS;
using Storm.Api.Dtos;
using Storm.Api.Extensions;
using Storm.Api.SourceGenerators.ActionMethods;

namespace MyApp.Domains.Users;

public static class UserErrors
{
    public const string EMAIL_TAKEN    = "EMAIL_TAKEN";
    public const string WEAK_PASSWORD  = "WEAK_PASSWORD";
    public const string TENANT_LOCKED  = "TENANT_LOCKED";
}

public class CreateUserParameter
{
    public required string Email    { get; init; }
    public required string Password { get; init; }
}

public class UserDto
{
    public required Guid   Id    { get; init; }
    public required string Email { get; init; }
}

[Summary("Create a new user")]
[Description("Creates a user account, hashes the password and sends a confirmation email.")]
[Description("Idempotent on email — re-posting the same address returns the existing account.")]
[ErrorCode(UserErrors.EMAIL_TAKEN,   Description = "Email is already registered to another account")]
[ErrorCode(UserErrors.WEAK_PASSWORD, Description = "Password does not meet complexity rules")]
[HttpError(HttpStatusCode.Unauthorized, Description = "Caller is not authenticated")]
[HttpError(HttpStatusCode.Forbidden,    Description = "Caller cannot create users in this tenant")]
[InternalActionCall<SendWelcomeEmailCommand>]
public class CreateUserCommand(IServiceProvider services)
    : BaseAuthenticatedAction<CreateUserParameter, UserDto, CurrentUser>(services)
{
    protected override Task Authorize(CreateUserParameter parameter, CurrentUser account)
    {
        if (!account.CanCreateUsers)
            throw new DomainHttpCodeException(HttpStatusCode.Forbidden);
        return Task.CompletedTask;
    }

    protected override async Task<UserDto> Action(CreateUserParameter parameter, CurrentUser account)
    {
        var existing = await UseReadConnection(db => db.SingleAsync<UserEntity>(x => x.Email == parameter.Email));
        if (existing is not null)
            throw new DomainException(UserErrors.EMAIL_TAKEN, "This email is already registered.");

        if (parameter.Password.Length < 12)
            throw new DomainException(UserErrors.WEAK_PASSWORD, "Password must be at least 12 characters.");

        var entity = new UserEntity { Id = Guid.CreateVersion7(), Email = parameter.Email };
        await UseWriteConnection(db => db.InsertAsync(entity));

        await services.ExecuteAction<SendWelcomeEmailCommand, SendWelcomeEmailParameter, Unit>(
            new() { Email = entity.Email });

        return new UserDto { Id = entity.Id, Email = entity.Email };
    }
}
```

## Controller (untouched)

```csharp
public partial class UsersController(IServiceProvider services) : BaseController(services)
{
    [HttpPost]
    [WithAction<CreateUserCommand>]
    [Tags("Users")]
    public partial Task<ActionResult<Response<UserDto>>> CreateUser([FromBody] CreateUserParameter body);
}
```

The generator emits (roughly):

```csharp
[EndpointSummary("Create a new user")]
[EndpointDescription(@"Error Codes:
- EMAIL_TAKEN: Email is already registered to another account
- WEAK_PASSWORD: Password does not meet complexity rules

Creates a user account, hashes the password and sends a confirmation email.

Idempotent on email — re-posting the same address returns the existing account.
")]
[ProducesResponseType<Response<UserDto>>(200, "application/json")]
[ProducesResponseType<Response>(401, "application/problem+json", Description = "Caller is not authenticated")]
[ProducesResponseType<Response>(403, "application/problem+json", Description = "Caller cannot create users in this tenant")]
[OpenApiErrorCodes("EMAIL_TAKEN", "WEAK_PASSWORD")]
public partial async Task<ActionResult<Response<UserDto>>> CreateUser(CreateUserParameter body)
{
    // generated action invocation
}
```

## File download action

For actions returning `ApiFileResult`, declare the content type with `[MediaType]`:

```csharp
[Summary("Download a user's avatar as PNG")]
[MediaType("image/png")]
[HttpError(HttpStatusCode.NotFound, Description = "User has no avatar")]
public class GetAvatarQuery(IServiceProvider services)
    : BaseAction<GetAvatarParameter, ApiFileResult>(services) { ... }
```

The generator emits `[ProducesResponseType<FileResult>(200, "image/png")]` for the success response.

## Custom success status codes

Use `[SuccessCode]` to override the default `200`:

```csharp
[SuccessCode(201, Description = "User created")]
[SuccessCode(202, Description = "User creation queued for async processing")]
public class CreateUserCommand(IServiceProvider services)
    : BaseAction<CreateUserParameter, UserDto>(services) { ... }
```

## Reusing error codes via `[InternalActionCall<>]`

When an action delegates to another action, don't restate its errors — declare the call:

```csharp
[ErrorCode("ORDER_LOCKED", Description = "Order is read-only and cannot be modified")]
[InternalActionCall<ChargeCardCommand>]   // pulls in PAYMENT_DECLINED, CARD_EXPIRED, etc.
[InternalActionCall<SendReceiptCommand>]  // pulls in EMAIL_BOUNCED
public class CheckoutCommand(IServiceProvider services)
    : BaseAction<CheckoutParameter, ReceiptDto>(services) { ... }
```

Only `[ErrorCode]` and `[HttpError]` are inherited — `[Summary]` / `[Description]` are not.
