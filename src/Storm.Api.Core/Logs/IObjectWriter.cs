using System;
using System.Collections.Generic;

namespace Storm.Api.Core.Logs
{
	public interface IObjectWriter
	{
		IObjectWriter WriteProperty(string property, string value);
		IObjectWriter WriteProperty(string property, double value);
		IObjectWriter WriteProperty(string property, long value);
		IObjectWriter WriteProperty(string property, bool value);

		IObjectWriter WriteProperty(string property, TimeSpan value);
		IObjectWriter WriteProperty(string property, DateTime value);
		IObjectWriter WriteProperty(string property, DateTimeOffset value);

		IObjectWriter WriteObject(string property, Action<IObjectWriter> objectWriter);
		IObjectWriter WriteObject(string property, IDictionary<string, string> properties);
		IObjectWriter WriteArray(string property, Action<IArrayWriter> arrayWriter);
	}

	public interface IArrayWriter
	{
		IArrayWriter WriteValue(string value);
		IArrayWriter WriteValue(double value);
		IArrayWriter WriteValue(long value);
		IArrayWriter WriteValue(bool value);

		IArrayWriter WriteValue(TimeSpan value);
		IArrayWriter WriteValue(DateTime value);
		IArrayWriter WriteValue(DateTimeOffset value);

		IArrayWriter WriteObject(Action<IObjectWriter> objectWriter);
	}
}