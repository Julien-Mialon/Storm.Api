using Storm.Api.Launchers;

namespace Storm.Api.Sample;

public class Program
{
	public static void Main(string[] args)
	{
		DefaultLauncherOptions.UseVault = false;
		DefaultLauncherOptions.SetDatabaseDebug = false;

		DefaultLauncher<Startup>.RunWebHost(args);
	}
}