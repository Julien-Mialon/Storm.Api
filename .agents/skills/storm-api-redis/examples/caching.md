# Caching in an Action

```csharp
using Storm.Api.CQRS;
using Storm.Api.Redis;

public class GetUserProfileQuery(IServiceProvider services)
    : BaseAction<GetUserProfileParameter, UserProfileDto>(services)
{
    protected override async Task<UserProfileDto> Action(GetUserProfileParameter parameter)
    {
        var redis = Resolve<IRedisService>();
        string cacheKey = $"profile:{parameter.UserId}";

        string? cached = await redis.GetStringAsync(cacheKey);
        if (cached is not null)
        {
            return JsonSerializer.Deserialize<UserProfileDto>(cached)!;
        }

        var repo = Resolve<IUserRepository>();
        var user = await repo.GetById(parameter.UserId)
            ?? throw new DomainException("USER_NOT_FOUND", "User not found");

        var dto = new UserProfileDto { Id = user.Id, Name = user.Name };
        await redis.SetAsync(cacheKey, JsonSerializer.Serialize(dto), TimeSpan.FromMinutes(10));
        return dto;
    }
}
```
