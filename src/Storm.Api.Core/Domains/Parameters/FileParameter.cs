using System;
using System.IO;

namespace Storm.Api.Core.Domains.Parameters
{
	public class FileParameter : IDisposable
	{
		public string ContentType { get; set; }

		public string ContentDisposition { get; set; }

		public long Length { get; set; }

		public string Name { get; set; }

		public string FileName { get; set; }

		public Stream InputStream { get; set; }


		public void Dispose()
		{
			InputStream?.Dispose();
		}
	}
}