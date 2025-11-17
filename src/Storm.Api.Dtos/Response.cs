using System.Text.Json.Serialization;

namespace Storm.Api.Dtos;

public class Response
{
	[JsonPropertyName("is_success")]
	public bool IsSuccess { get; set; }

	[JsonPropertyName("error_code")]
	public string? ErrorCode { get; set; }

	[JsonPropertyName("error_message")]
	public string? ErrorMessage { get; set; }
}

public class Response<T> : Response
{
	[JsonPropertyName("data")]
	public T? Data { get; set; }
}