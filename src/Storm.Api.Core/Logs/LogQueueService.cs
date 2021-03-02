using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Storm.Api.Core.Logs
{
	public interface ILogQueueService
	{
		Task<string> Next(TimeSpan timeout);
	}

	public class LogQueueService : LogService, ILogQueueService, ILogSender
	{
		private readonly ConcurrentQueue<string> _logs = new ConcurrentQueue<string>();
		private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);

		public LogQueueService(LogLevel minimumLogLevel) : base(minimumLogLevel)
		{
			UseSender(this);
		}

		public void Enqueue(LogLevel level, string entry)
		{
			_logs.Enqueue(entry);
			_semaphore.Release();
		}

		public async Task<string> Next(TimeSpan timeout)
		{
			if (!await _semaphore.WaitAsync(timeout))
			{
				return null;
			}
			if (_logs.TryDequeue(out string result))
			{
				return result;
			}

			_semaphore.Release();
			return null;
		}
	}
}