using System.IO;

namespace Storm.Api.Core.Domains.Results
{
	public abstract class FileResult
	{
		public string FileName { get; set; }

		public string ContentType { get; set; }

		public abstract bool IsRawData { get; }

		public abstract bool IsStreamData { get; }

		public abstract byte[] AsRawData();

		public abstract Stream AsStreamData();

		public static FileResult Create(byte[] data, string contentType, string filename = null) => new FileByteResult
		{
			Data = data,
			ContentType = contentType,
			FileName = filename
		};

		public static FileResult Create(Stream data, string contentType, string filename = null) => new FileStreamResult
		{
			Data = data,
			ContentType = contentType,
			FileName = filename
		};
	}
}