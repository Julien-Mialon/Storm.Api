using System.IO;

namespace Storm.Api.Core.Domains.Results
{
	public class FileStreamResult : BaseFileResult
	{
		public Stream Data { get; set; }
	}
}