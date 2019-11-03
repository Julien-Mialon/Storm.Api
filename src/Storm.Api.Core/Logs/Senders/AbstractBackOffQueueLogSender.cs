using System;
using System.Text;
using System.Threading.Tasks;
using Storm.Api.Core.Workers;

namespace Storm.Api.Core.Logs.Senders
{
	public abstract class AbstractBackOffQueueLogSender : ILogSender
	{
		private readonly BackgroundItemExponentialQueueWorker<string> _worker;

		protected AbstractBackOffQueueLogSender(ILogService logService)
		{
			_worker = new BackgroundItemExponentialQueueWorker<string>(logService, Send);
		}

		public void Enqueue(LogLevel level, string entry)
		{
			_worker.Queue(entry);
		}

		protected abstract Task<bool> Send(string entry);
	}
}
