using System.Globalization;
using System.Text;
using System.Text.Json;
using Storm.Api.Logs.Interfaces;

namespace Storm.Api.Logs.Internals;

internal class JsonLogWriter : IObjectWriter, IArrayWriter, IDisposable, IAsyncDisposable
{
	private readonly bool _extraLine;
	private readonly MemoryStream _memoryStream = new();
	private readonly Utf8JsonWriter _jsonWriter;

	public JsonLogWriter(bool indented = false, bool extraLine = false)
	{
		_extraLine = extraLine;

		_jsonWriter = new(_memoryStream, new() { Indented = indented });
		_jsonWriter.WriteStartObject();
	}

	IArrayWriter IArrayWriter.WriteValue(string? value)
	{
		_jsonWriter.WriteStringValue(value);
		return this;
	}

	IArrayWriter IArrayWriter.WriteValue(double value)
	{
		_jsonWriter.WriteNumberValue(value);
		return this;
	}

	IArrayWriter IArrayWriter.WriteValue(long value)
	{
		_jsonWriter.WriteNumberValue(value);
		return this;
	}

	IArrayWriter IArrayWriter.WriteValue(bool value)
	{
		_jsonWriter.WriteBooleanValue(value);
		return this;
	}

	IArrayWriter IArrayWriter.WriteValue(DateTime value)
	{
		_jsonWriter.WriteStringValue(value.ToString("o", CultureInfo.InvariantCulture));
		return this;
	}

	IArrayWriter IArrayWriter.WriteValue(DateTimeOffset value)
	{
		_jsonWriter.WriteStringValue(value.ToString("o", CultureInfo.InvariantCulture));
		return this;
	}

	IObjectWriter IObjectWriter.WriteProperty(string property, string? value)
	{
		_jsonWriter.WritePropertyName(property);
		_jsonWriter.WriteStringValue(value);
		return this;
	}

	IObjectWriter IObjectWriter.WriteProperty(string property, double value)
	{
		_jsonWriter.WritePropertyName(property);
		_jsonWriter.WriteNumberValue(value);
		return this;
	}

	IObjectWriter IObjectWriter.WriteProperty(string property, long value)
	{
		_jsonWriter.WritePropertyName(property);
		_jsonWriter.WriteNumberValue(value);
		return this;
	}

	IObjectWriter IObjectWriter.WriteProperty(string property, bool value)
	{
		_jsonWriter.WritePropertyName(property);
		_jsonWriter.WriteBooleanValue(value);
		return this;
	}

	IObjectWriter IObjectWriter.WriteProperty(string property, DateTime value)
	{
		_jsonWriter.WritePropertyName(property);
		_jsonWriter.WriteStringValue(value.ToString("o", CultureInfo.InvariantCulture));
		return this;
	}

	IObjectWriter IObjectWriter.WriteProperty(string property, DateTimeOffset value)
	{
		_jsonWriter.WritePropertyName(property);
		_jsonWriter.WriteStringValue(value.ToString("o", CultureInfo.InvariantCulture));
		return this;
	}

	IObjectWriter IObjectWriter.WriteObject(string property, Action<IObjectWriter> objectWriter)
	{
		_jsonWriter.WritePropertyName(property);
		_jsonWriter.WriteStartObject();

		objectWriter.Invoke(this);

		_jsonWriter.WriteEndObject();
		return this;
	}

	IObjectWriter IObjectWriter.WriteObject(string property, IDictionary<string, string> properties)
	{
		_jsonWriter.WritePropertyName(property);
		_jsonWriter.WriteStartObject();
		foreach (KeyValuePair<string, string> it in properties)
		{
			_jsonWriter.WritePropertyName(it.Key);
			_jsonWriter.WriteStringValue(it.Value);
		}
		_jsonWriter.WriteEndObject();
		return this;
	}

	IObjectWriter IObjectWriter.WriteArray(string property, Action<IArrayWriter> arrayWriter)
	{
		_jsonWriter.WritePropertyName(property);
		_jsonWriter.WriteStartArray();

		arrayWriter.Invoke(this);

		_jsonWriter.WriteEndArray();
		return this;
	}

	IArrayWriter IArrayWriter.WriteObject(Action<IObjectWriter> objectWriter)
	{
		_jsonWriter.WriteStartObject();

		objectWriter.Invoke(this);

		_jsonWriter.WriteEndObject();
		return this;
	}

	public override string ToString()
	{
		_jsonWriter.WriteEndObject();
		_jsonWriter.Flush();

		string result = Encoding.UTF8.GetString(_memoryStream.ToArray());

		if (_extraLine)
		{
			result += Environment.NewLine;
		}

		return result;
	}

	public void Dispose()
	{
		_memoryStream.Dispose();
		_jsonWriter.Dispose();
	}

	public async ValueTask DisposeAsync()
	{
		await _memoryStream.DisposeAsync();
		await _jsonWriter.DisposeAsync();
	}
}