using System.Data;
using ServiceStack.OrmLite.Converters;

namespace Storm.Api.Databases.Converters;

public class SqlServerDateOnlyConverter : DateOnlyConverter
{
	private const string STRING_INPUT_FORMAT = "yyyy-MM-dd";
	private const string DB_FORMAT = "yyyy-MM-dd";
	public override DbType DbType => DbType.Date;
	public override string ColumnDefinition => "DATE";

	public override object ToDbValue(Type fieldType, object value)
	{
		if (value is DateOnly dateOnly)
		{
			return dateOnly.ToString(DB_FORMAT);
		}

		if (value is string str && DateOnly.TryParseExact(str, STRING_INPUT_FORMAT, out dateOnly))
		{
			return dateOnly.ToString(DB_FORMAT);
		}

		return base.ToDbValue(fieldType, value);
	}

	public override object FromDbValue(Type fieldType, object value)
	{
		if (value is DateOnly dateOnly)
		{
			return dateOnly;
		}

		if (value is DateTime dateTime)
		{
			return new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
		}

		if (value is string str && DateOnly.TryParseExact(str, DB_FORMAT, out dateOnly))
		{
			return dateOnly;
		}

		return base.FromDbValue(fieldType, value);
	}

	public override string ToQuotedString(Type fieldType, object value)
	{
		if (value is DateOnly dateOnly)
		{
			return DialectProvider.GetQuotedValue(dateOnly.ToString(DB_FORMAT));
		}

		return base.ToQuotedString(fieldType, value);
	}
}