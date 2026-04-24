using ServiceStack.OrmLite;
using Storm.Api.Databases.Converters;

namespace Storm.Api.Tests.Databases;

public class SqlServerDateOnlyConverterTests
{
	private static SqlServerDateOnlyConverter Make()
	{
		SqlServerDateOnlyConverter conv = new();
		conv.DialectProvider = SqlServerDialect.Provider;
		return conv;
	}

	[Fact]
	public void ToDbValue_ReturnsIsoFormattedString()
	{
		object val = Make().ToDbValue(typeof(DateOnly), new DateOnly(2024, 6, 1));
		val.Should().Be("2024-06-01");
	}

	[Fact]
	public void FromDbValue_StringInput_ParsesToDateOnly()
	{
		object val = Make().FromDbValue(typeof(DateOnly), "2024-06-01");
		val.Should().Be(new DateOnly(2024, 6, 1));
	}

	[Fact]
	public void FromDbValue_DateTimeInput_ConvertsToDateOnly()
	{
		object val = Make().FromDbValue(typeof(DateOnly), new DateTime(2024, 6, 1, 12, 34, 56));
		val.Should().Be(new DateOnly(2024, 6, 1));
	}

	[Fact]
	public void ToQuotedString_WrapsValueInSingleQuotes()
	{
		string q = Make().ToQuotedString(typeof(DateOnly), new DateOnly(2024, 6, 1));
		q.Should().Contain("2024-06-01");
		q.Should().Contain("'");
	}

	[Fact]
	public void RoundTrip_PreservesDateExactly()
	{
		SqlServerDateOnlyConverter c = Make();
		DateOnly original = new(2024, 6, 1);
		object db = c.ToDbValue(typeof(DateOnly), original);
		object back = c.FromDbValue(typeof(DateOnly), db);
		back.Should().Be(original);
	}

	[Fact]
	public void FromDbValue_InvalidFormat_Throws()
	{
		Action act = () => Make().FromDbValue(typeof(DateOnly), "not-a-date");
		act.Should().Throw<FormatException>();
	}
}
