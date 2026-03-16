namespace Storm.Api.CQRS.Domains.Results;

internal class FileByteResult : ApiFileResult
{
	public byte[] Data { get; init; }

	public override bool IsRawData => true;

	public override bool IsStreamData => false;

	public FileByteResult(string fileName, string contentType, byte[] data) : base(fileName, contentType)
	{
		Data = data;
	}

	public override byte[] AsRawData()
	{
		return Data;
	}

	public override Stream AsStreamData()
	{
		throw new InvalidOperationException("Stream data not supported");
	}
}