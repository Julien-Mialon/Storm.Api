using System.Threading.Tasks;
using Storm.Api.Core.Logs.Senders;

namespace Storm.Api.Core.Logs.ElasticSearch.Senders
{
	internal class ImmediateQueueLogSender : AbstractImmediateQueueLogSender
	{
		private readonly ElasticSender _client;

		public ImmediateQueueLogSender(ILogService service, ElasticSender client) : base(service)
		{
			_client = client;
		}

		protected override Task<bool> Send(string entry)
		{
			return _client.Send(entry);
		}
	}
}