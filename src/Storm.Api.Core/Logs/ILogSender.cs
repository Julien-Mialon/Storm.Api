namespace Storm.Api.Core.Logs
{
	public interface ILogSender
	{
		void Enqueue(LogLevel level, string entry);
	}
}