namespace Storm.Api.Extensions;

public static class NullableExtensions
{
	public delegate bool TryParseDelegate<in TInput, TOutput>(TInput value, out TOutput result);

	public static void Let<T>(this T? value, Action<T> action) where T : struct
	{
		if (value.HasValue)
		{
			action(value.Value);
		}
	}

	public static void Let<T>(this T? value, Action<T> action) where T : class
	{
		if (value is not null)
		{
			action(value);
		}
	}

	public static void LetIf<T>(this T? value, Func<T, bool> condition, Action<T> action) where T : struct
	{
		if (value.HasValue)
		{
			action(value.Value);
		}
	}

	public static void LetIf<T>(this T? value, Func<T, bool> condition, Action<T> action) where T : class
	{
		if (value is not null)
		{
			action(value);
		}
	}

	public static void LetParseEnum<TEnum>(this string? value, Action<TEnum> action) where TEnum : struct
	{
		if (value is not null && Enum.TryParse(value, true, out TEnum parsed))
		{
			action(parsed);
		}
	}
}