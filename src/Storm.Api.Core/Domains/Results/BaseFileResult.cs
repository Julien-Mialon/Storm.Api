namespace Storm.Api.Core.Domains.Results
{
	public abstract class BaseFileResult
	{
		public string FileName { get; set; }
		
		public string ContentType { get; set; }
	}
}