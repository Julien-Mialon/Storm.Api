namespace Storm.Api.Core.Domains.Results;

internal class FileStreamResult : FileResult
{
	public Stream? Data { get; set; }

	public override bool IsRawData => false;
	public override bool IsStreamData => true;
	public override byte[] AsRawData() => throw new InvalidOperationException("Raw data not supported");
	public override Stream AsStreamData() => Data!;
}