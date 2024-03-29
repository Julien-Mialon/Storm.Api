namespace Storm.Api;

public enum EnvironmentSlot
{
	Local,
	Dev,
	Test,
	Alpha,
	Beta,
	Prod,
}

public static class EnvironmentHelper
{
	public static EnvironmentSlot Slot { get; private set; }

	public static void Set(EnvironmentSlot slot)
	{
		Slot = slot;
	}

	public static void SetFromEnvironment(string environmentName)
	{
		int delimiterIndex = environmentName.LastIndexOf('-');
		if (delimiterIndex >= 0)
		{
			environmentName = environmentName.Substring(delimiterIndex + 1);
		}

		Set(environmentName switch
		{
			"dev" => EnvironmentSlot.Dev,
			"test" => EnvironmentSlot.Test,
			"alpha" => EnvironmentSlot.Alpha,
			"beta" => EnvironmentSlot.Beta,
			"prod" => EnvironmentSlot.Prod,
			_ => EnvironmentSlot.Local,
		});
	}

	public static bool IsAvailableClient => Slot == EnvironmentSlot.Alpha || Slot == EnvironmentSlot.Beta || Slot == EnvironmentSlot.Prod;

	public static bool IsInternal => Slot == EnvironmentSlot.Dev || Slot == EnvironmentSlot.Test;

	public static bool IsLocal => Slot == EnvironmentSlot.Local;
}