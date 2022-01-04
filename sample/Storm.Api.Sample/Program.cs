using Storm.Api.Launchers;

namespace Storm.Api.Sample;

public class Program
{
	public static void Main(string[] args)
	{
		DefaultLauncher<Startup>.RunWebHost(args);
	}
}