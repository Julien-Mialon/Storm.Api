using Microsoft.AspNetCore.Http;
using Storm.Api.Core.Domains.Parameters;

namespace Storm.Api.Extensions;

public static class FormFileExtensions
{
	public static async Task<FileParameter?> ToFileParameter(this IFormFile? file, bool copyStream = false)
	{
		if (file == null || file.Length == 0)
		{
			return null;
		}

		Stream fileStream;

		if (copyStream)
		{
			MemoryStream memoryStream = new MemoryStream((int)file.Length);
			await file.CopyToAsync(memoryStream);
			memoryStream.Seek(0, SeekOrigin.Begin);

			fileStream = memoryStream;
		}
		else
		{
			fileStream = file.OpenReadStream();
		}

		return new FileParameter
		{
			ContentDisposition = file.ContentDisposition,
			ContentType = file.ContentType,
			FileName = file.FileName,
			Length = file.Length,
			Name = file.Name,
			InputStream = fileStream,
		};
	}
}