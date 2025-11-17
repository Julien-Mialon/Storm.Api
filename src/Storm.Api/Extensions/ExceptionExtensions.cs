using Microsoft.Data.SqlClient;

namespace Storm.Api.Extensions;

public static class ExceptionExtensions
{
	public static bool IsDatabaseException(this Exception ex)
	{
		return ex is SqlException
		       || (ex is InvalidOperationException invalidOperationException && (
			       invalidOperationException.Message.Contains("The connection's current state is closed.")
			       || invalidOperationException.Message.Contains("The connection is closed.")
			       || (invalidOperationException.StackTrace?.Contains("ServiceStack.OrmLite.OrmLiteExecFilter") ?? false)
			       || (invalidOperationException.StackTrace?.Contains("AdapterUtil.SqlClient") ?? false)
		       ))
		       || (ex is NullReferenceException nullReferenceException && (nullReferenceException.StackTrace?.Contains("ServiceStack.OrmLite.OrmLiteExecFilter") ?? false))
		       || (ex is AggregateException aggregateException && (
				       (aggregateException.InnerException is not null && aggregateException.InnerException.IsDatabaseException())
				       || aggregateException.InnerExceptions.Any(x => x.IsDatabaseException())
			       )
		       );
	}
}