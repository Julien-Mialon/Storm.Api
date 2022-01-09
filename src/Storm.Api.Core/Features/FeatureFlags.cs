namespace Storm.Api.Core.Features;

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
		if (FLAGS.ContainsKey(flag))
		{
			return;
		}

		FLAGS[flag] = value;
	}

	public static bool IsEnabled(string flag)
	{
		if (FLAGS.TryGetValue(flag, out bool value))
		{
			return value;
		}

		return false;
	}

	public static bool IsDisabled(string flag) => !IsEnabled(flag);
}