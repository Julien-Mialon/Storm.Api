using ServiceStack.Logging;

namespace Storm.Api.Databases;

internal class LogServiceLogFactory : ILogFactory
{
	private readonly bool _debugEnabled;

	public LogServiceLogFactory(bool debugEnabled)
	{
		_debugEnabled = debugEnabled;
	}

	public ILog GetLogger(Type type)
	{
		return new LogServiceDatabaseLog(_debugEnabled, type.Name);
	}

	public ILog GetLogger(string typeName)
	{
		return new LogServiceDatabaseLog(_debugEnabled, typeName);
	}
}