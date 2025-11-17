using System.Runtime.CompilerServices;
using Storm.Api.Logs.Interfaces;

namespace Storm.Api.Logs.Extensions;

public static class ObjectWriterExtensions
{
	public static IObjectWriter WriteException(this IObjectWriter writer, Exception? exception, string property = "exception")
	{
		if (exception == null)
		{
			return writer;
		}

		return writer.WriteObject(property, x => InternalWriteException(x, exception));
	}

	private static void InternalWriteException(IObjectWriter writer, Exception exception)
	{
		writer.WriteProperty("message", exception.Message)
			.WriteProperty("type", exception.GetType().FullName)
			.WriteProperty("stacktrace", exception.StackTrace);
		if (exception is AggregateException aggregateException)
		{
			writer.WriteArray("inner_exceptions", array =>
			{
				foreach (Exception innerException in aggregateException.InnerExceptions)
				{
					array.WriteObject(x => InternalWriteException(x, innerException));
				}
			});
		}
		else if (exception.InnerException != null)
		{
			writer.WriteException(exception.InnerException, "inner_exception");
		}
	}

	public static IObjectWriter WriteMethodInfo(this IObjectWriter writer, string property = "caller", [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
	{
		return writer.WriteObject(property, x =>
			x.WriteProperty("file", file)
				.WriteProperty("line", line)
				.WriteProperty("member", member));
	}

	public static IObjectWriter WriteMessage(this IObjectWriter writer, string message)
	{
		return writer.WriteProperty("message", message);
	}
}