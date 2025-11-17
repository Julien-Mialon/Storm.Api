using System.Data;
using ServiceStack.OrmLite.Dapper;

namespace Storm.Api.Databases.Converters.DapperMappers;

public class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
{
	private const string DB_FORMAT = "HH:mm:ss.fff";

	public override void SetValue(IDbDataParameter parameter, TimeOnly value)
	{
		parameter.DbType = DbType.Time; // no force of SqlParameter.SqlDbType needed because Microsoft.Data.SqlClient is used instead of System.Data.SqlClient
		parameter.Value = value.ToString(DB_FORMAT);
	}

	public override TimeOnly Parse(object value)
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

		throw new InvalidOperationException($"Unable to parse TimeOnly from value {value} of type {value.GetType()}");
	}
}