namespace Storm.Api.Core.Extensions;

public static class CollectionExtensions
{
	public static List<TOutput> ConvertAll<TInput, TOutput>(this IEnumerable<TInput> source, Func<TInput, TOutput> mapper)
	{
		source.ThrowArgumentNullExceptionIfNeeded(nameof(source));
		mapper.ThrowArgumentNullExceptionIfNeeded(nameof(mapper));

		return new(source.Select(mapper));
	}

	public static List<TOutput> ConvertAll<TInput, TOutput>(this ICollection<TInput> source, Func<TInput, TOutput> mapper)
	{
		source.ThrowArgumentNullExceptionIfNeeded(nameof(source));
		mapper.ThrowArgumentNullExceptionIfNeeded(nameof(mapper));

		List<TOutput> result = new(source.Count);
		result.AddRange(source.Select(mapper));
		return result;
	}

	public static async Task<List<TOutput>> ConvertAll<TInput, TOutput>(this Task<IEnumerable<TInput>> source, Func<TInput, TOutput> mapper) => (await source).ConvertAll(mapper);

	public static async Task<List<TOutput>> ConvertAll<TInput, TOutput>(this Task<ICollection<TInput>> source, Func<TInput, TOutput> mapper) => (await source).ConvertAll(mapper);

	public static async Task<List<TOutput>> ConvertAll<TInput, TOutput>(this Task<IList<TInput>> source, Func<TInput, TOutput> mapper) => (await source).ConvertAll(mapper);

	public static async Task<List<TOutput>> ConvertAll<TInput, TOutput>(this Task<List<TInput>> source, Func<TInput, TOutput> mapper) => (await source).ConvertAll(mapper);

	public static bool None<T>(this IEnumerable<T> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		return !source.Any();
	}

	public static bool None<T>(this ICollection<T> source)
	{
		if (source == null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		return source.Count == 0;
	}

	public static bool None<T>(this IEnumerable<T> source, Func<T, bool> predicate)
	{
		if (source == null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		return !source.Any(predicate);
	}
}