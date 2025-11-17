namespace Storm.Api.Logs.Interfaces;

public interface ILogAppender
{
	bool MultipleAllowed { get; }

	void Append(IObjectWriter logEntry);
}