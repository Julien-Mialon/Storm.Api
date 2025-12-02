using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Storm.Api.Dtos;

public class PaginatedResponse<T> : Response
{
	[JsonPropertyName("data")]
	[JsonProperty("data")]
	public T[]? Data { get; set; }

	[JsonPropertyName("page")]
	[JsonProperty("page")]
	public int Page { get; set; }

	[JsonPropertyName("count")]
	[JsonProperty("count")]
	public int Count { get; set; }

	[JsonPropertyName("total_count")]
	[JsonProperty("total_count")]
	public int TotalCount { get; set; }
}