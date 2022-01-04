namespace Storm.Api.Core.Logs.Consoles;

public class ConsoleLogSender : ILogSender
{
	public void Enqueue(LogLevel level, string entry)
	{
		Console.WriteLine($"[{level}] {entry}");
	}
}