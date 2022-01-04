namespace Storm.Api.Core.Domains.Parameters;

public class FileParameter : IDisposable
{
	public string ContentType { get; set; } = string.Empty;

	public string ContentDisposition { get; set; } = string.Empty;

	public long Length { get; set; }

	public string Name { get; set; } = string.Empty;

	public string FileName { get; set; } = string.Empty;

	public Stream? InputStream { get; set; }


	public void Dispose()
	{
		InputStream?.Dispose();
	}
}