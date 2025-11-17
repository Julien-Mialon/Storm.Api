namespace Storm.Api.Extensions;

public static class TasksExtensions
{
	public static Task<T> AsTask<T>(this T result)
	{
		return Task.FromResult(result);
	}
	public static Task<T?> AsTaskNullable<T>(this T result) where T : struct
	{
		return Task.FromResult<T?>(result);
	}

	public static Task<T?> AsTaskNullable<T>(this T? result) where T : struct
	{
		return Task.FromResult(result);
	}

	public static Task WaitForCancellation(this CancellationToken cancellationToken)
	{
		if(cancellationToken.IsCancellationRequested)
		{
			return Task.CompletedTask;
		}

		TaskCompletionSource<bool> tcs = new();
		cancellationToken.Register(s => ((TaskCompletionSource<bool>)s!).TrySetResult(true), tcs);

		return tcs.Task;
	}
}