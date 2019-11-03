namespace Storm.Api.Dtos
{
	public class Response
	{
		public bool IsSuccess { get; set; }

		public string ErrorCode { get; set; }

		public string ErrorMessage { get; set; }
	}

	public class Response<T> : Response
	{
		public T Data { get; set; }
	}
}