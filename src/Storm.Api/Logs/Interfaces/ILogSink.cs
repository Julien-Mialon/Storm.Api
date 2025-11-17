using Microsoft.Extensions.Logging;

namespace Storm.Api.Logs.Interfaces;

public interface ILogSink
{
	void Enqueue(LogLevel level, string entry);
}