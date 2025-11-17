namespace Storm.Api.CQRS.Domains.Results;

internal class FileStreamResult : ApiFileResult
{
	public Stream Data { get; init; }

	public FileStreamResult(string fileName, string contentType, Stream data) : base(fileName, contentType)
	{
		Data = data;
	}

	public override bool IsRawData => false;
	public override bool IsStreamData => true;
	public override byte[] AsRawData() => throw new InvalidOperationException("Raw data not supported");
	public override Stream AsStreamData() => Data;
}