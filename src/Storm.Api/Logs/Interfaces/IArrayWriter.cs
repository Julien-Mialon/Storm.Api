namespace Storm.Api.Logs.Interfaces;

public interface IArrayWriter
{
	IArrayWriter WriteValue(string? value);
	IArrayWriter WriteValue(double value);
	IArrayWriter WriteValue(long value);
	IArrayWriter WriteValue(bool value);
	IArrayWriter WriteValue(DateTime value);
	IArrayWriter WriteValue(DateTimeOffset value);

	IArrayWriter WriteObject(Action<IObjectWriter> objectWriter);
}