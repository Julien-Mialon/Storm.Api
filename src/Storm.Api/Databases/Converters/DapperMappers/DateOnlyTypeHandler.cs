using System.Data;
using ServiceStack.OrmLite.Dapper;

namespace Storm.Api.Databases.Converters.DapperMappers;

public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
	private const string DB_FORMAT = "yyyy-MM-dd";

	public override void SetValue(IDbDataParameter parameter, DateOnly value)
	{
		parameter.DbType = DbType.Date; // no force of SqlParameter.SqlDbType needed because Microsoft.Data.SqlClient is used instead of System.Data.SqlClient
		parameter.Value = value.ToString(DB_FORMAT);
	}

	public override DateOnly Parse(object value)
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

		throw new InvalidOperationException($"Unable to parse DateOnly from value {value} of type {value.GetType()}");
	}
}