using Storm.Api.CQRS.Domains.Results;

namespace Storm.Api.Tests.CQRS.Domains;

public class FileContentTypeTests
{
	[Theory]
	[InlineData("CONTENT_TYPE_PDF", "application/pdf")]
	[InlineData("CONTENT_TYPE_PNG", "image/png")]
	[InlineData("CONTENT_TYPE_JPG", "image/jpeg")]
	[InlineData("CONTENT_TYPE_EXCEL", "application/octet-stream")]
	[InlineData("CONTENT_TYPE_ZIP", "application/zip")]
	[InlineData("CONTENT_TYPE_JSON", "application/json")]
	public void Constants_MatchExpectedMimeTypes(string constName, string expected)
	{
		System.Reflection.FieldInfo? field = typeof(FileContentType).GetField(constName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
		field.Should().NotBeNull();
		field!.GetValue(null).Should().Be(expected);
	}
}
