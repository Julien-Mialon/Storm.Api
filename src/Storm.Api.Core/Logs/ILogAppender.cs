namespace Storm.Api.Core.Logs;

public interface ILogAppender
{
	void Append(IObjectWriter logEntry);
}