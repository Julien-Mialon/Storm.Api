using System.Threading.Tasks;
using Storm.Api.Core.Logs.Senders;

namespace Storm.Api.Core.Logs.ElasticSearch.Senders
{
	internal class BackOffQueueLogSender : AbstractBackOffQueueLogSender
	{
		private readonly ElasticSender _client;

		public BackOffQueueLogSender(ILogService logService, ElasticSender client) : base(logService)
		{
			_client = client;
		}

		protected override Task<bool> Send(string entry)
		{
			return _client.Send(entry);
		}
	}
}