using Microsoft.AspNetCore.Http;

namespace Storm.Api.CQRS.Domains.Parameters;

public class FileParameter : IDisposable, IAsyncDisposable
{
	public bool IsEmpty => InputStream == null || Length == 0;

	public string ContentType { get; set; } = string.Empty;

	public string ContentDisposition { get; set; } = string.Empty;

	public long Length { get; set; }

	public string Name { get; set; } = string.Empty;

	public string FileName { get; set; } = string.Empty;

	public Stream? InputStream { get; set; }

	public FileParameter()
	{
	}

	public async Task<FileParameter> LoadFrom(IFormFile? file, bool copyStream = false)
	{
		if (file == null || file.Length == 0)
		{
			return this;
		}

		Stream fileStream;

		if (copyStream)
		{
			MemoryStream memoryStream = new((int)file.Length);
			await file.CopyToAsync(memoryStream);
			memoryStream.Seek(0, SeekOrigin.Begin);

			fileStream = memoryStream;
		}
		else
		{
			fileStream = file.OpenReadStream();
		}

		ContentDisposition = file.ContentDisposition;
		ContentType = file.ContentType;
		FileName = file.FileName;
		Length = file.Length;
		Name = file.Name;
		InputStream = fileStream;

		return this;
	}

	public void Dispose()
	{
		InputStream?.Dispose();
	}

	public async ValueTask DisposeAsync()
	{
		if (InputStream is not null)
		{
			await InputStream.DisposeAsync();
		}
	}
}