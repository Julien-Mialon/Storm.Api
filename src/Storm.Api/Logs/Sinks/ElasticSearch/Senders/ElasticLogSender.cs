using System.Text;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using HttpMethod = Elastic.Transport.HttpMethod;

namespace Storm.Api.Logs.Sinks.ElasticSearch.Senders;

public interface IElasticSender
{
	Task<bool> Send(string entry);
	Task<bool> Send(IReadOnlyList<string> entries);
}

internal class ElasticLogSender : IElasticSender
{
	private readonly ElasticsearchClient _client;
	private readonly string _index;

	private string _currentIndex;
	private DateTime _indexExpirationDate;

	public ElasticLogSender(ElasticsearchClient client, string index)
	{
		_client = client;
		_index = index;

		if (_index.Contains("{year}") || _index.Contains("{month}") || _index.Contains("{day}"))
		{
			_currentIndex = index;
			UpdateCurrentIndex();
		}
		else
		{
			_currentIndex = _index;
			_indexExpirationDate = DateTime.UtcNow.AddYears(100);
		}
	}

	public async Task<bool> Send(string entry)
	{
		UpdateCurrentIndex();

		EndpointPath endpointPath = new(HttpMethod.POST, $"/{_currentIndex}/_create");
		StringResponse result = await _client.Transport.RequestAsync<StringResponse>(endpointPath, PostData.String(entry));

		if (result.ApiCallDetails.HttpStatusCode is >= 200 and < 300)
		{
			return true;
		}

		return false;
	}

	public async Task<bool> Send(IReadOnlyList<string> entries)
	{
		UpdateCurrentIndex();
		StringBuilder content = new(entries.Count * (200 + 75));

		foreach (string entry in entries)
		{
			content.Append($"{{\"index\":{{}}");
			content.Append("\n");
			content.Append(entry);
			content.Append("\n");
		}

		string body = content.ToString();

		EndpointPath endpointPath = new(HttpMethod.POST, $"/{_currentIndex}/_bulk");
		StringResponse result = await _client.Transport.RequestAsync<StringResponse>(endpointPath, PostData.String(body));

		// StringResponse result = await _client.BulkAsync<StringResponse>(_currentIndex, PostData.String(body));

		if (result.ApiCallDetails.HttpStatusCode is >= 200 and < 300)
		{
			return true;
		}

		return false;
	}

	private void UpdateCurrentIndex()
	{
		DateTime date = DateTime.UtcNow;
		if (date < _indexExpirationDate)
		{
			return;
		}

		_currentIndex = _index.Replace("{year}", date.ToString("yyyy"))
			.Replace("{month}", date.ToString("MM"))
			.Replace("{day}", date.ToString("dd"));
		_indexExpirationDate = date.AddMinutes(5);
	}
}