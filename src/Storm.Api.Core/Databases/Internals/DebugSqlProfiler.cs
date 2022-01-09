using System.Data.Common;
using System.Diagnostics;
using ServiceStack.MiniProfiler.Data;

namespace Storm.Api.Core.Databases.Internals;

internal class DebugSqlProfiler : IDbProfiler
{
	public void ExecuteStart(DbCommand profiledDbCommand, ExecuteType executeType)
	{
		Debug.WriteLine(profiledDbCommand.CommandText);
	}

	public void ExecuteFinish(DbCommand profiledDbCommand, ExecuteType executeType, DbDataReader reader)
	{

	}

	public void ReaderFinish(DbDataReader reader)
	{

	}

	public void OnError(DbCommand profiledDbCommand, ExecuteType executeType, Exception exception)
	{

	}

	public bool IsActive { get; } = true;
}