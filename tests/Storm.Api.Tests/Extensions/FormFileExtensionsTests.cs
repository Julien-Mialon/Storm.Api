using Microsoft.AspNetCore.Http;
using Storm.Api.CQRS.Domains.Parameters;
using Storm.Api.Extensions;

namespace Storm.Api.Tests.Extensions;

public class FormFileExtensionsTests
{
	private static FormFile MakeFormFile(byte[] data, string name = "file", string fileName = "test.txt", string contentType = "text/plain")
	{
		MemoryStream ms = new(data);
		return new FormFile(ms, 0, data.Length, name, fileName)
		{
			Headers = new HeaderDictionary(),
			ContentType = contentType,
			ContentDisposition = $"form-data; name=\"{name}\"; filename=\"{fileName}\"",
		};
	}

	[Fact]
	public async Task ToFileParameter_NullFile_ReturnsNull()
	{
		IFormFile? file = null;
		(await file.ToFileParameter()).Should().BeNull();
	}

	[Fact]
	public async Task ToFileParameter_EmptyFile_ReturnsNull()
	{
		FormFile f = MakeFormFile([]);
		(await f.ToFileParameter()).Should().BeNull();
	}

	[Fact]
	public async Task ToFileParameter_CopyStreamFalse_UsesDirectStream()
	{
		FormFile f = MakeFormFile([1, 2, 3]);
		FileParameter? p = await f.ToFileParameter(copyStream: false);
		p.Should().NotBeNull();
		p!.InputStream.Should().NotBeNull();
	}

	[Fact]
	public async Task ToFileParameter_CopyStreamTrue_CopiesToMemoryStream()
	{
		FormFile f = MakeFormFile([1, 2, 3]);
		FileParameter? p = await f.ToFileParameter(copyStream: true);
		p!.InputStream.Should().BeOfType<MemoryStream>();
	}

	[Fact]
	public async Task ToFileParameter_PropagatesAllMetadataFields()
	{
		FormFile f = MakeFormFile([1, 2, 3], name: "upload", fileName: "f.bin", contentType: "x/y");
		FileParameter? p = await f.ToFileParameter();
		p!.Name.Should().Be("upload");
		p.FileName.Should().Be("f.bin");
		p.ContentType.Should().Be("x/y");
		p.Length.Should().Be(3);
	}
}
