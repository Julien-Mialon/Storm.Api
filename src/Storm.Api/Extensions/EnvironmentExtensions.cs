using Microsoft.Extensions.Hosting;

namespace Storm.Api.Extensions;

public static class EnvironmentExtensions
{
	public static string SimpleEnvironmentName(this IHostEnvironment environment)
	{
		string name = environment.EnvironmentName;
		int delimiterIndex = name.LastIndexOf('-');
		if (delimiterIndex > 0)
		{
			name = name[..delimiterIndex];
		}

		return name;
	}
}