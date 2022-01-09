namespace Storm.Api.Core.Extensions;

public static class TasksExtensions
{
	public static Task<T> AsTask<T>(this T result)
	{
		return Task.FromResult(result);
	}
}