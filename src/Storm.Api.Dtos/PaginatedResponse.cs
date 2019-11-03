namespace Storm.Api.Dtos
{
	public class PaginatedResponse<T> : Response
	{
		public T[] Data { get; set; }

		public int Page { get; set; }

		public int Count { get; set; }

		public int TotalCount { get; set; }
	}
}