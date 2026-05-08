using Storm.Api.Logs.Interfaces;

namespace Storm.Api.Logs.Appenders;

public sealed class TimestampLogAppender : ILogAppender
{
	public static string DefaultTimestampFieldName { get; set; } = "timestamp";

	private readonly Func<DateTime> _timestamp;
	private readonly string _fieldName;

	public bool MultipleAllowed => false;

	public TimestampLogAppender(string? fieldName = null) : this(TimeProvider.System, fieldName)
	{
	}

	public TimestampLogAppender(TimeProvider timeProvider, string? fieldName = null)
	{
		_timestamp = () => timeProvider.GetUtcNow().UtcDateTime;
		_fieldName = fieldName ?? DefaultTimestampFieldName;
	}

	public TimestampLogAppender(Func<DateTime> timestamp, string? fieldName = null)
	{
		_timestamp = timestamp;
		_fieldName = fieldName ?? DefaultTimestampFieldName;
	}

	public void Append(IObjectWriter writer)
	{
		writer.WriteProperty(_fieldName, _timestamp());
	}
}