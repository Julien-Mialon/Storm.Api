using ServiceStack.OrmLite;
using Storm.Api.Databases.Converters;

namespace Storm.Api.Tests.Databases;

public class SqlServerTimeOnlyConverterTests
{
	private static SqlServerTimeOnlyConverter Make()
	{
		SqlServerTimeOnlyConverter c = new();
		c.DialectProvider = SqlServerDialect.Provider;
		return c;
	}

	[Fact]
	public void ToDbValue_ReturnsHhMmSsFormat()
	{
		object val = Make().ToDbValue(typeof(TimeOnly), new TimeOnly(10, 11, 12));
		val.Should().Be("10:11:12.000");
	}

	[Fact]
	public void FromDbValue_StringInput_ParsesToTimeOnly()
	{
		object val = Make().FromDbValue(typeof(TimeOnly), "10:11:12.000");
		val.Should().Be(new TimeOnly(10, 11, 12));
	}

	[Fact]
	public void FromDbValue_TimeSpanInput_ConvertsToTimeOnly()
	{
		object val = Make().FromDbValue(typeof(TimeOnly), new TimeSpan(10, 11, 12));
		val.Should().Be(new TimeOnly(10, 11, 12));
	}

	[Fact]
	public void ToQuotedString_WrapsValueInSingleQuotes()
	{
		string q = Make().ToQuotedString(typeof(TimeOnly), new TimeOnly(10, 11, 12));
		q.Should().Contain("10:11:12");
		q.Should().Contain("'");
	}

	[Fact]
	public void RoundTrip_PreservesTimeExactly()
	{
		SqlServerTimeOnlyConverter c = Make();
		TimeOnly original = new(23, 59, 59);
		object db = c.ToDbValue(typeof(TimeOnly), original);
		object back = c.FromDbValue(typeof(TimeOnly), db);
		back.Should().Be(original);
	}

	[Fact]
	public void BoundaryValues_Midnight_And_EndOfDay_Handled()
	{
		SqlServerTimeOnlyConverter c = Make();
		TimeOnly midnight = new(0, 0, 0);
		TimeOnly end = new(23, 59, 59);
		c.ToDbValue(typeof(TimeOnly), midnight).Should().Be("00:00:00.000");
		c.ToDbValue(typeof(TimeOnly), end).Should().Be("23:59:59.000");
	}
}
