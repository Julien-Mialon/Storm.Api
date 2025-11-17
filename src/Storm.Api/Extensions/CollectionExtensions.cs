using System.Runtime.CompilerServices;

namespace Storm.Api.Extensions;

public static class CollectionExtensions
{
	public static List<TOutput> ConvertAll<TInput, TOutput>(this IEnumerable<TInput> source, Func<TInput, TOutput> mapper)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(mapper);

		return new(source.Select(mapper));
	}

	public static List<TOutput> ConvertAll<TInput, TOutput>(this ICollection<TInput> source, Func<TInput, TOutput> mapper)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(mapper);

		List<TOutput> result = new(source.Count);
		result.AddRange(source.Select(mapper));
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async Task<List<TOutput>> ConvertAll<TInput, TOutput>(this Task<IEnumerable<TInput>> source, Func<TInput, TOutput> mapper) => (await source).ConvertAll(mapper);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async Task<List<TOutput>> ConvertAll<TInput, TOutput>(this Task<ICollection<TInput>> source, Func<TInput, TOutput> mapper) => (await source).ConvertAll(mapper);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async Task<List<TOutput>> ConvertAll<TInput, TOutput>(this Task<IList<TInput>> source, Func<TInput, TOutput> mapper) => (await source).ConvertAll(mapper);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async Task<List<TOutput>> ConvertAll<TInput, TOutput>(this Task<List<TInput>> source, Func<TInput, TOutput> mapper) => (await source).ConvertAll(mapper);

	public static bool None<T>(this IEnumerable<T> source)
	{
		ArgumentNullException.ThrowIfNull(source);

		return !source.Any();
	}

	public static bool None<T>(this ICollection<T> source)
	{
		ArgumentNullException.ThrowIfNull(source);

		return source.Count == 0;
	}

	public static bool None<T>(this IEnumerable<T> source, Func<T, bool> predicate)
	{
		ArgumentNullException.ThrowIfNull(source);

		return !source.Any(predicate);
	}

	public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> items)
	{
		foreach (T item in items)
		{
			set.Add(item);
		}
	}

	public static Dictionary<TKey, TElement> ToSafeDictionary<TSource, TKey, TElement>(this IEnumerable<TSource>? source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector) where TKey : notnull
	{
		Dictionary<TKey, TElement> result = new Dictionary<TKey, TElement>();

		if (source != null)
		{
			foreach (TSource item in source)
			{
				result[keySelector(item)] = elementSelector(item);
			}
		}

		return result;
	}

	public static List<T> ToListOrDefault<T>(this IEnumerable<T>? source)
	{
		return source?.ToList() ?? new List<T>();
	}

	public static int IndexOfMin<T, TKey>(this IReadOnlyList<T> source, Func<T, TKey> selector, IComparer<TKey>? comparer = null)
	{
		if (source.Count <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(source), "Invalid source, must be a minimum of 1 item");
		}

		comparer ??= Comparer<TKey>.Default;

		TKey minValue = selector(source[0]);
		int minIndex = 0;

		for (int i = 1; i < source.Count; i++)
		{
			TKey value = selector(source[i]);
			if (comparer.Compare(value, minValue) < 0)
			{
				minIndex = i;
				minValue = value;
			}
		}

		return minIndex;
	}

	public static int IndexOfMax<T, TKey>(this IReadOnlyList<T> source, Func<T, TKey> selector, IComparer<TKey>? comparer = null)
	{
		if (source.Count <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(source), "Invalid source, must be a minimum of 1 item");
		}

		comparer ??= Comparer<TKey>.Default;

		TKey maxValue = selector(source[0]);
		int maxIndex = 0;

		for (int i = 1; i < source.Count; i++)
		{
			TKey value = selector(source[i]);
			if (comparer.Compare(value, maxValue) > 0)
			{
				maxIndex = i;
				maxValue = value;
			}
		}

		return maxIndex;
	}
}