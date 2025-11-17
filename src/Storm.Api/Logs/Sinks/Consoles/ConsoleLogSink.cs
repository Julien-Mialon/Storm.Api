using Microsoft.Extensions.Logging;
using Storm.Api.Logs.Interfaces;

namespace Storm.Api.Logs.Sinks.Consoles;

public class ConsoleLogSink : ILogSink
{
	public void Enqueue(LogLevel level, string entry)
	{
		Console.WriteLine($"[{level}] {entry}");
	}
}