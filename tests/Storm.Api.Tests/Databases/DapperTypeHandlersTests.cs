using System.Data;
using System.Data.Common;
using Storm.Api.Databases.Converters.DapperMappers;

namespace Storm.Api.Tests.Databases;

public class DapperTypeHandlersTests
{
	private sealed class FakeParam : IDbDataParameter
	{
		public DbType DbType { get; set; }
		public ParameterDirection Direction { get; set; }
		public bool IsNullable { get; set; }
		public string ParameterName { get; set; } = "";
		public string SourceColumn { get; set; } = "";
		public DataRowVersion SourceVersion { get; set; }
		public object? Value { get; set; }
		public byte Precision { get; set; }
		public byte Scale { get; set; }
		public int Size { get; set; }
	}

	[Fact]
	public void DateOnly_SetValue_AssignsDateTimeParameter()
	{
		DateOnlyTypeHandler h = new();
		FakeParam p = new();
		h.SetValue(p, new DateOnly(2024, 6, 1));
		p.DbType.Should().Be(DbType.Date);
		p.Value.Should().Be("2024-06-01");
	}

	[Fact]
	public void DateOnly_Parse_DateTimeInput_ReturnsDateOnly()
	{
		new DateOnlyTypeHandler().Parse(new DateTime(2024, 6, 1)).Should().Be(new DateOnly(2024, 6, 1));
	}

	[Fact]
	public void DateOnly_Parse_NullInput_Throws()
	{
		// Current impl throws on unknown types (including null-equivalent)
		Action act = () => new DateOnlyTypeHandler().Parse(123);
		act.Should().Throw<InvalidOperationException>();
	}

	[Fact]
	public void TimeOnly_SetValue_AssignsTimeSpanParameter()
	{
		TimeOnlyTypeHandler h = new();
		FakeParam p = new();
		h.SetValue(p, new TimeOnly(10, 11, 12));
		p.DbType.Should().Be(DbType.Time);
		p.Value.Should().Be("10:11:12.000");
	}

	[Fact]
	public void TimeOnly_Parse_TimeSpanInput_ReturnsTimeOnly()
	{
		new TimeOnlyTypeHandler().Parse(new TimeSpan(10, 11, 12)).Should().Be(new TimeOnly(10, 11, 12));
	}

	[Fact]
	public void TimeOnly_Parse_UnknownInput_Throws()
	{
		Action act = () => new TimeOnlyTypeHandler().Parse(123);
		act.Should().Throw<InvalidOperationException>();
	}
}
