using Microsoft.AspNetCore.Http;
using Storm.Api.CQRS.Domains.Parameters;

namespace Storm.Api.Tests.CQRS.Domains;

public class FileParameterTests
{
	private static FormFile MakeFormFile(byte[] data, string name = "file", string fileName = "test.txt", string contentType = "text/plain")
	{
		MemoryStream ms = new(data);
		FormFile f = new(ms, 0, data.Length, name, fileName)
		{
			Headers = new HeaderDictionary(),
			ContentType = contentType,
			ContentDisposition = $"form-data; name=\"{name}\"; filename=\"{fileName}\"",
		};
		return f;
	}

	[Fact]
	public void IsEmpty_NullInputStream_ReturnsTrue()
	{
		new FileParameter().IsEmpty.Should().BeTrue();
	}

	[Fact]
	public void IsEmpty_LengthZero_ReturnsTrue()
	{
		using MemoryStream ms = new();
		FileParameter p = new() { InputStream = ms, Length = 0 };
		p.IsEmpty.Should().BeTrue();
	}

	[Fact]
	public void IsEmpty_StreamAndNonZeroLength_ReturnsFalse()
	{
		using MemoryStream ms = new([1]);
		FileParameter p = new() { InputStream = ms, Length = 1 };
		p.IsEmpty.Should().BeFalse();
	}

	[Fact]
	public async Task LoadFrom_NullFile_ReturnsSelfUntouched()
	{
		FileParameter p = new();
		FileParameter result = await p.LoadFrom(null);
		result.Should().BeSameAs(p);
		result.InputStream.Should().BeNull();
	}

	[Fact]
	public async Task LoadFrom_EmptyFile_ReturnsSelfUntouched()
	{
		FormFile f = MakeFormFile([]);
		FileParameter p = new();
		FileParameter result = await p.LoadFrom(f);
		result.Should().BeSameAs(p);
		result.InputStream.Should().BeNull();
	}

	[Fact]
	public async Task LoadFrom_CopyStreamFalse_UsesOpenedReadStream()
	{
		FormFile f = MakeFormFile([1, 2, 3]);
		FileParameter p = new();
		await p.LoadFrom(f, copyStream: false);
		p.InputStream.Should().NotBeNull();
	}

	[Fact]
	public async Task LoadFrom_CopyStreamTrue_CreatesMemoryStreamAtPositionZero()
	{
		FormFile f = MakeFormFile([1, 2, 3]);
		FileParameter p = new();
		await p.LoadFrom(f, copyStream: true);

		p.InputStream.Should().BeOfType<MemoryStream>();
		p.InputStream!.Position.Should().Be(0);
	}

	[Fact]
	public async Task LoadFrom_CopiesAllPropertiesFromFormFile()
	{
		FormFile f = MakeFormFile([1, 2, 3], name: "upload", fileName: "hello.bin", contentType: "application/octet-stream");
		FileParameter p = new();
		await p.LoadFrom(f);

		p.Name.Should().Be("upload");
		p.FileName.Should().Be("hello.bin");
		p.ContentType.Should().Be("application/octet-stream");
		p.Length.Should().Be(3);
		p.ContentDisposition.Should().Contain("hello.bin");
	}

	[Fact]
	public async Task LoadFrom_ReturnsThis_SupportsChaining()
	{
		FileParameter p = new();
		FileParameter result = await p.LoadFrom(null);
		result.Should().BeSameAs(p);
	}

	[Fact]
	public void Dispose_DisposesInputStream()
	{
		MemoryStream ms = new([1]);
		FileParameter p = new() { InputStream = ms, Length = 1 };
		p.Dispose();
		Action act = () => ms.ReadByte();
		act.Should().Throw<ObjectDisposedException>();
	}

	[Fact]
	public void Dispose_NullInputStream_IsNoOp()
	{
		FileParameter p = new();
		p.Dispose();
	}

	[Fact]
	public async Task DisposeAsync_DisposesInputStream()
	{
		MemoryStream ms = new([1]);
		FileParameter p = new() { InputStream = ms, Length = 1 };
		await p.DisposeAsync();
		Action act = () => ms.ReadByte();
		act.Should().Throw<ObjectDisposedException>();
	}
}
