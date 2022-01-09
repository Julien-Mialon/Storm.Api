using System.Globalization;
using Newtonsoft.Json;

namespace Storm.Api.Core.Logs.Internals;

internal class JsonLogWriter : IObjectWriter, IArrayWriter
{
	private readonly bool _extraLine;
	private readonly StringWriter _textWriter = new();
	private readonly JsonWriter _jsonWriter;

	public JsonLogWriter(Formatting formatting = Formatting.None, bool extraLine = false)
	{
		_extraLine = extraLine;

		_jsonWriter = new JsonTextWriter(_textWriter) { Formatting = formatting };
		_jsonWriter.WriteStartObject();
	}

	IArrayWriter IArrayWriter.WriteValue(string? value)
	{
		_jsonWriter.WriteValue(value);
		return this;
	}

	IArrayWriter IArrayWriter.WriteValue(double value)
	{
		_jsonWriter.WriteValue(value);
		return this;
	}

	IArrayWriter IArrayWriter.WriteValue(long value)
	{
		_jsonWriter.WriteValue(value);
		return this;
	}

	IArrayWriter IArrayWriter.WriteValue(bool value)
	{
		_jsonWriter.WriteValue(value);
		return this;
	}

	IArrayWriter IArrayWriter.WriteValue(DateTime value)
	{
		_jsonWriter.WriteValue(value.ToString("o", CultureInfo.InvariantCulture));
		return this;
	}

	IArrayWriter IArrayWriter.WriteValue(DateTimeOffset value)
	{
		_jsonWriter.WriteValue(value.ToString("o", CultureInfo.InvariantCulture));
		return this;
	}

	IArrayWriter IArrayWriter.WriteValue(TimeSpan value)
	{
		_jsonWriter.WriteValue(value);
		return this;
	}

	IObjectWriter IObjectWriter.WriteProperty(string property, string? value)
	{
		_jsonWriter.WritePropertyName(property);
		_jsonWriter.WriteValue(value);
		return this;
	}

	IObjectWriter IObjectWriter.WriteProperty(string property, double value)
	{
		_jsonWriter.WritePropertyName(property);
		_jsonWriter.WriteValue(value);
		return this;
	}

	IObjectWriter IObjectWriter.WriteProperty(string property, long value)
	{
		_jsonWriter.WritePropertyName(property);
		_jsonWriter.WriteValue(value);
		return this;
	}

	IObjectWriter IObjectWriter.WriteProperty(string property, bool value)
	{
		_jsonWriter.WritePropertyName(property);
		_jsonWriter.WriteValue(value);
		return this;
	}

	IObjectWriter IObjectWriter.WriteProperty(string property, DateTime value)
	{
		_jsonWriter.WritePropertyName(property);
		_jsonWriter.WriteValue(value.ToString("o", CultureInfo.InvariantCulture));
		return this;
	}

	IObjectWriter IObjectWriter.WriteProperty(string property, DateTimeOffset value)
	{
		_jsonWriter.WritePropertyName(property);
		_jsonWriter.WriteValue(value.ToString("o", CultureInfo.InvariantCulture));
		return this;
	}

	IObjectWriter IObjectWriter.WriteProperty(string property, TimeSpan value)
	{
		_jsonWriter.WritePropertyName(property);
		_jsonWriter.WriteValue(value);
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
			_jsonWriter.WriteValue(it.Value);
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

		string result = _textWriter.ToString();

		if (_extraLine)
		{
			result += Environment.NewLine;
		}

		return result;
	}
}