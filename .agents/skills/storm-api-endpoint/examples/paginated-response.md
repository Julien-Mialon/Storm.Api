# PaginatedResponse Example

The action's output type is `PaginatedResponse<T>` directly — the framework detects it as a `Response` subtype and sets `IsSuccess = true` automatically without double-wrapping.

## Action

```csharp
public class ListUsersParameter
{
    public required int Page { get; init; }
    public required int Count { get; init; }
}

public class ListUsersQuery(IServiceProvider services)
    : BaseAction<ListUsersParameter, PaginatedResponse<UserDto>>(services)
{
    protected override async Task<PaginatedResponse<UserDto>> Action(ListUsersParameter parameter)
    {
        var total = await UseReadConnection(db => db.CountAsync<UserEntity>());
        var items = await UseReadConnection(db => db.SelectAsync<UserEntity>(
            db.From<UserEntity>()
              .Skip((parameter.Page - 1) * parameter.Count)
              .Take(parameter.Count)));

        return new PaginatedResponse<UserDto>
        {
            Data = items.Select(x => new UserDto { Id = x.Id, Name = x.Name }).ToArray(),
            Page = parameter.Page,
            Count = items.Count,
            TotalCount = (int)total,
        };
    }
}
```

## Controller

```csharp
[HttpGet]
[WithAction<ListUsersQuery>]
public partial Task<ActionResult<PaginatedResponse<UserDto>>> ListUsers(
    [FromQuery] int page,
    [FromQuery] int count);
```

## Response format

```json
{ "is_success": true, "data": [...], "page": 1, "count": 20, "total_count": 150 }
```
