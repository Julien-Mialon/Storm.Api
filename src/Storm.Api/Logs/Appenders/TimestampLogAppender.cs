using Storm.Api.Logs.Interfaces;

namespace Storm.Api.Logs.Appenders;

public sealed class TimestampLogAppender : ILogAppender
{
	private readonly Func<DateTime> _timestamp;
	private readonly string _fieldName;

	public bool MultipleAllowed => false;

	public TimestampLogAppender(string fieldName = "timestamp") : this(() => DateTime.UtcNow, fieldName)
	{
	}

	public TimestampLogAppender(Func<DateTime> timestamp, string fieldName = "timestamp")
	{
		_timestamp = timestamp;
		_fieldName = fieldName;
	}

	public void Append(IObjectWriter writer)
	{
		writer.WriteProperty(_fieldName, _timestamp());
	}
}