namespace Storm.Api.Core.Domains.Results;

public abstract class FileResult
{
	public string FileName { get; set; } = string.Empty;

	public string ContentType { get; set; } = string.Empty;

	public abstract bool IsRawData { get; }

	public abstract bool IsStreamData { get; }

	public abstract byte[] AsRawData();

	public abstract Stream AsStreamData();

	public static FileResult Create(byte[] data, string contentType, string filename = "file") => new FileByteResult
	{
		Data = data,
		ContentType = contentType,
		FileName = filename
	};

	public static FileResult Create(Stream data, string contentType, string filename = "file") => new FileStreamResult
	{
		Data = data,
		ContentType = contentType,
		FileName = filename
	};
}