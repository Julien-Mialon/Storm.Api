using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Storm.Api.Dtos;

public class Response
{
	[JsonPropertyName("is_success")]
	[JsonProperty("is_success")]
	public bool IsSuccess { get; set; }

	[JsonPropertyName("error_code")]
	[JsonProperty("error_code")]
	public string? ErrorCode { get; set; }

	[JsonPropertyName("error_message")]
	[JsonProperty("error_message")]
	public string? ErrorMessage { get; set; }
}

public class Response<T> : Response
{
	[JsonPropertyName("data")]
	[JsonProperty("data")]
	public T? Data { get; set; }
}