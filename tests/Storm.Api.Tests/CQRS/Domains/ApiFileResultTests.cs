using Storm.Api.CQRS.Domains.Results;

namespace Storm.Api.Tests.CQRS.Domains;

public class ApiFileResultTests
{
	[Fact]
	public void Create_FromBytes_ReturnsFileByteResult()
	{
		ApiFileResult result = ApiFileResult.Create([1, 2, 3], "image/png", "a.png");
		result.IsRawData.Should().BeTrue();
		result.IsStreamData.Should().BeFalse();
	}

	[Fact]
	public void Create_FromStream_ReturnsFileStreamResult()
	{
		using MemoryStream ms = new([1, 2, 3]);
		ApiFileResult result = ApiFileResult.Create(ms, "image/png", "a.png");
		result.IsStreamData.Should().BeTrue();
		result.IsRawData.Should().BeFalse();
	}

	[Fact]
	public void Create_DefaultFileNameIsFile()
	{
		ApiFileResult.Create([], "x").FileName.Should().Be("file");
	}

	[Fact]
	public void Create_ContentTypeStoredOnInit()
	{
		ApiFileResult.Create([], "text/plain").ContentType.Should().Be("text/plain");
	}

	[Fact]
	public void FileByteResult_IsRawDataTrue_IsStreamDataFalse()
	{
		ApiFileResult r = ApiFileResult.Create([9], "x");
		r.IsRawData.Should().BeTrue();
		r.IsStreamData.Should().BeFalse();
	}

	[Fact]
	public void FileByteResult_AsRawData_ReturnsOriginalBytes()
	{
		byte[] data = [1, 2, 3];
		ApiFileResult.Create(data, "x").AsRawData().Should().Equal(data);
	}

	[Fact]
	public void FileByteResult_AsStreamData_ThrowsInvalidOperation()
	{
		Action act = () => ApiFileResult.Create([], "x").AsStreamData();
		act.Should().Throw<InvalidOperationException>();
	}

	[Fact]
	public void FileStreamResult_IsStreamDataTrue_IsRawDataFalse()
	{
		using MemoryStream ms = new();
		ApiFileResult r = ApiFileResult.Create(ms, "x");
		r.IsStreamData.Should().BeTrue();
		r.IsRawData.Should().BeFalse();
	}

	[Fact]
	public void FileStreamResult_AsStreamData_ReturnsOriginalStream()
	{
		using MemoryStream ms = new();
		ApiFileResult r = ApiFileResult.Create(ms, "x");
		r.AsStreamData().Should().BeSameAs(ms);
	}

	[Fact]
	public void FileStreamResult_AsRawData_ThrowsInvalidOperation()
	{
		using MemoryStream ms = new();
		Action act = () => ApiFileResult.Create(ms, "x").AsRawData();
		act.Should().Throw<InvalidOperationException>();
	}
}
