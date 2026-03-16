namespace Storm.Api.CQRS.Domains.Results;

internal class FileStreamResult : ApiFileResult
{
	public Stream Data { get; init; }

	public override bool IsRawData => false;

	public override bool IsStreamData => true;

	public FileStreamResult(string fileName, string contentType, Stream data) : base(fileName, contentType)
	{
		Data = data;
	}

	public override byte[] AsRawData()
	{
		throw new InvalidOperationException("Raw data not supported");
	}

	public override Stream AsStreamData()
	{
		return Data;
	}
}