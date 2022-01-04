namespace Storm.Api.Core.Domains.Results;

public class PaginatedResult<T>
{
	public int Page { get; set; }

	public int Count { get; set; }

	public int TotalCount { get; set; }

	public T[]? Data { get; set; }
}