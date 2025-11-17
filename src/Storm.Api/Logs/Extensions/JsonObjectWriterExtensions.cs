using System.Text.Json;
using Storm.Api.Logs.Interfaces;

namespace Storm.Api.Logs.Extensions;

public static class JsonObjectWriterExtensions
{
	public static IObjectWriter DumpObject(this IObjectWriter writer, string property, object? obj)
	{
		if (obj is null)
		{
			return writer.WriteProperty(property, null);
		}

		return writer.WriteProperty(property, JsonSerializer.Serialize(obj));
	}
}