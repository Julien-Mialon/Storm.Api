using System.Collections.Generic;

namespace Storm.Api.Core.Features
{
	public static class FeatureFlags
	{
		private static readonly Dictionary<string, bool> _flags = new Dictionary<string, bool>();

		public static void Set(Dictionary<string, bool> flags)
		{
			foreach (KeyValuePair<string,bool> flag in flags)
			{
				_flags[flag.Key] = flag.Value;
			}
		}

		public static void Set(string flag, bool value)
		{
			_flags[flag] = value;
		}

		public static void SetDefault(string flag, bool value)
		{
			if (_flags.ContainsKey(flag))
			{
				return;
			}

			_flags[flag] = value;
		}

		public static bool IsEnabled(string flag)
		{
			if (_flags.TryGetValue(flag, out bool value))
			{
				return value;
			}

			return false;
		}

		public static bool IsDisabled(string flag) => !IsEnabled(flag);
	}
}