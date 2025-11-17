namespace Storm.Api.Extensions;

public static class RandomExtensions
{
	private static readonly char[] ALPHA_CHARS =
	[
		'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
		'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
	];

	private static readonly char[] ALPHA_DIGITS_CHARS =
	[
		'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
		'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
		'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
	];

	public static string RandomAlphaString(this int length) => RandomString(length, ALPHA_CHARS);

	public static string RandomAlphaDigitsString(this int length) => RandomString(length, ALPHA_DIGITS_CHARS);

	public static string RandomString(this int length, char[] charset)
	{
		char[] chars = new char[length];
		for (int i = 0; i < length; i++)
		{
			chars[i] = charset[System.Random.Shared.Next(charset.Length)];
		}

		return new string(chars);
	}

	public static int Random(this int maxValue)
	{
		return System.Random.Shared.Next(maxValue);
	}

	public static long Random(this long maxValue)
	{
		return (long) (System.Random.Shared.NextDouble() * maxValue);
	}

	public static T RandomItem<T>(this List<T> items)
	{
		int index = System.Random.Shared.Next(items.Count);
		return items[index];
	}
}