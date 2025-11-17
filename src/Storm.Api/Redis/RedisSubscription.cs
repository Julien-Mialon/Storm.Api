using System.Runtime.CompilerServices;
using StackExchange.Redis;

namespace Storm.Api.Redis;

public class RedisSubscription : IDisposable, IAsyncDisposable
{
	private readonly ChannelMessageQueue _queue;

	public RedisSubscription(ChannelMessageQueue queue)
	{
		_queue = queue;
	}

	public async IAsyncEnumerable<string> Read([EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		try
		{
			await using IAsyncEnumerator<ChannelMessage> asyncEnumerator = _queue.GetAsyncEnumerator(cancellationToken);
			while (await asyncEnumerator.MoveNextAsync())
			{
				ChannelMessage message = asyncEnumerator.Current;
				string? data = message.Message;
				if (data is not null)
				{
					yield return data;
				}
			}
		}
		finally
		{
			await _queue.UnsubscribeAsync();
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore().ConfigureAwait(false);

		Dispose(disposing: false);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_queue.Unsubscribe();
		}
	}

	protected virtual async ValueTask DisposeAsyncCore()
	{
		await _queue.UnsubscribeAsync();
	}
}