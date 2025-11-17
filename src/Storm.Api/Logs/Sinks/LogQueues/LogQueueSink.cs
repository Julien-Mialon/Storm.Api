using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Storm.Api.Logs.Interfaces;

namespace Storm.Api.Logs.Sinks.LogQueues;

public class LogQueueSink : ILogQueueSink, ILogSink
{
	private readonly Channel<string> _channel = Channel.CreateUnbounded<string>();

	public void Enqueue(LogLevel level, string entry)
	{
		_channel.Writer.TryWrite(entry);
	}

	public async Task<string?> Next(TimeSpan timeout)
	{
		CancellationTokenSource cts = new(timeout);
		try
		{
			return await _channel.Reader.ReadAsync(cts.Token);
		}
		catch (Exception)
		{
			return null;
		}
	}
}