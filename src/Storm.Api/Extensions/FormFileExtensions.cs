using Microsoft.AspNetCore.Http;
using Storm.Api.CQRS.Domains.Parameters;

namespace Storm.Api.Extensions;

public static class FormFileExtensions
{
	public static async Task<FileParameter?> ToFileParameter(this IFormFile? file, bool copyStream = false)
	{
		if (file is { Length: > 0 })
		{
			return await new FileParameter()
				.LoadFrom(file, copyStream);
		}

		return null;
	}
}