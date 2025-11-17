namespace Storm.Api.CQRS.Domains.Results;

public abstract class ApiFileResult
{
	public string FileName { get; init; }

	public string ContentType { get; init; }

	public abstract bool IsRawData { get; }

	public abstract bool IsStreamData { get; }

	protected ApiFileResult(string fileName, string contentType)
	{
		FileName = fileName;
		ContentType = contentType;
	}

	public abstract byte[] AsRawData();

	public abstract Stream AsStreamData();

	public static ApiFileResult Create(byte[] data, string contentType, string fileName = "file")
	{
		return new FileByteResult(fileName, contentType, data);
	}

	public static ApiFileResult Create(Stream data, string contentType, string fileName = "file")
	{
		return new FileStreamResult(fileName, contentType, data);
	}
}