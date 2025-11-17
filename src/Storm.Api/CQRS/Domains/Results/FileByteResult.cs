namespace Storm.Api.CQRS.Domains.Results;

internal class FileByteResult : ApiFileResult
{
	public byte[] Data { get; init; }

	public FileByteResult(string fileName, string contentType, byte[] data) : base(fileName, contentType)
	{
		Data = data;
	}

	public override bool IsRawData => true;
	public override bool IsStreamData => false;
	public override byte[] AsRawData() => Data;
	public override Stream AsStreamData() => throw new InvalidOperationException("Stream data not supported");
}