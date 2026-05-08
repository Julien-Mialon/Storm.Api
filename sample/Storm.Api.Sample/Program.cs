using Storm.Api.Launchers;
using Storm.Api.Logs.Appenders;

namespace Storm.Api.Sample;

public class Program
{
	public static void Main(string[] args)
	{
		DefaultLauncherOptions.UseVault = false;
		DefaultLauncherOptions.SetDatabaseDebug = false;
		TimestampLogAppender.DefaultTimestampFieldName = "@timestamp";

		DefaultLauncher<Startup>.RunWebHost(args);
	}
}