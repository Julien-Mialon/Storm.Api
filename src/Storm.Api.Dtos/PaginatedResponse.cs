using Newtonsoft.Json;

namespace Storm.Api.Dtos;

public class PaginatedResponse<T> : Response
{
	[JsonProperty("data")]
	public T[]? Data { get; set; }

	[JsonProperty("page")]
	public int Page { get; set; }

	[JsonProperty("count")]
	public int Count { get; set; }

	[JsonProperty("total_count")]
	public int TotalCount { get; set; }
}