using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;

namespace Storm.Api.Core.Logs.ElasticSearch.Senders
{
	internal class ElasticSender
	{
		private readonly ElasticLowLevelClient _client;
		private readonly string _index;
		private readonly string _type;

		public ElasticSender(ElasticLowLevelClient client, string index, string type)
		{
			_client = client;
			_index = index;
			_type = type;
		}

		public async Task<bool> Send(string entry)
		{
			StringResponse result = await _client.IndexAsync<StringResponse>(_index, _type, entry);

			if (result.HttpStatusCode is int statusCode && statusCode >= 200 && statusCode < 300)
			{
				return true;
			}

			return false;
		}

		public async Task<bool> Send(IReadOnlyList<string> entries)
		{
			StringBuilder content = new StringBuilder(entries.Count * (200 + 75));

			foreach (string entry in entries)
			{
				content.AppendLine($"{{\"index\":{{\"_index\":\"{_index}\", \"_type\":\"{_type}\"}}");
				content.AppendLine(entry);
			}

			StringResponse result = await _client.BulkAsync<StringResponse>(content.ToString());

			if (result.HttpStatusCode is int statusCode && statusCode >= 200 && statusCode < 300)
			{
				return true;
			}

			return false;
		}
	}
}