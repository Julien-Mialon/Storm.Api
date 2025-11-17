using System.Data;
using ServiceStack.OrmLite;

namespace Storm.Api.Databases.Converters;

public class SqlServerTimeOnlyConverter : OrmLiteConverter
{
	private const string STRING_INPUT_FORMAT = "HH:mm:ss";
	private const string DB_FORMAT = "HH:mm:ss.fff";
	public override DbType DbType => DbType.Time;
	public override string ColumnDefinition => "TIME(3)";

	public override object ToDbValue(Type fieldType, object value)
	{
		if (value is TimeOnly timeOnly)
		{
			return timeOnly.ToString(DB_FORMAT);
		}

		if (value is string str && TimeOnly.TryParseExact(str, STRING_INPUT_FORMAT, out timeOnly))
		{
			return timeOnly.ToString(DB_FORMAT);
		}

		return base.ToDbValue(fieldType, value);
	}

	public override object FromDbValue(Type fieldType, object value)
	{
		if (value is TimeOnly timeOnly)
		{
			return timeOnly;
		}

		if (value is TimeSpan timeSpan)
		{
			return new TimeOnly(timeSpan.Ticks);
		}

		if (value is string str && TimeOnly.TryParseExact(str, DB_FORMAT, out timeOnly))
		{
			return timeOnly;
		}

		return base.FromDbValue(fieldType, value);
	}

	public override string ToQuotedString(Type fieldType, object value)
	{
		if (value is TimeOnly timeOnly)
		{
			return DialectProvider.GetQuotedValue(timeOnly.ToString(DB_FORMAT));
		}

		return base.ToQuotedString(fieldType, value);
	}
}