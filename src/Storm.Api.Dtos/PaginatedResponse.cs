using System.Text.Json.Serialization;

namespace Storm.Api.Dtos;

public class PaginatedResponse<T> : Response
{
	[JsonPropertyName("data")]
	public T[]? Data { get; set; }

	[JsonPropertyName("page")]
	public int Page { get; set; }

	[JsonPropertyName("count")]
	public int Count { get; set; }

	[JsonPropertyName("total_count")]
	public int TotalCount { get; set; }
}