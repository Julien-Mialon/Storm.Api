using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;

namespace Storm.Api.Features;

public static class FeatureFlags
{
	private static readonly Dictionary<string, bool> FLAGS = new();

	public static void Set(Dictionary<string, bool> flags)
	{
		foreach (KeyValuePair<string,bool> flag in flags)
		{
			FLAGS[flag.Key] = flag.Value;
		}
	}

	public static void Set(string flag, bool value)
	{
		FLAGS[flag] = value;
	}

	public static void SetDefault(string flag, bool value)
	{
		FLAGS.TryAdd(flag, value);
	}

	public static bool IsEnabled(string flag)
	{
		return FLAGS.GetValueOrDefault(flag, false);
	}

	public static bool IsDisabled(string flag) => !IsEnabled(flag);

	public static void LoadFromConfiguration(IConfiguration configuration)
	{
		Dictionary<string, bool> flags = configuration.GetChildren()
			.ToDictionary(x => x.Key, x => x.Get<bool>());
		Set(flags);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void LoadFeatureFlags(this IConfiguration configuration)
	{
		LoadFromConfiguration(configuration);
	}
}