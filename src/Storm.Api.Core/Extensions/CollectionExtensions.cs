using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Storm.Api.Core.Extensions
{
	public static class CollectionExtensions
	{
		public static List<TOutput> ConvertAll<TInput, TOutput>(this IEnumerable<TInput> source, Func<TInput, TOutput> mapper)
		{
			source.ThrowArgumentNullExceptionIfNeeded(nameof(source));
			mapper.ThrowArgumentNullExceptionIfNeeded(nameof(mapper));

			return new List<TOutput>(source.Select(mapper));
		}

		public static List<TOutput> ConvertAll<TInput, TOutput>(this ICollection<TInput> source, Func<TInput, TOutput> mapper)
		{
			source.ThrowArgumentNullExceptionIfNeeded(nameof(source));
			mapper.ThrowArgumentNullExceptionIfNeeded(nameof(mapper));

			List<TOutput> result = new List<TOutput>(source.Count);
			result.AddRange(source.Select(mapper));
			return result;
		}

		public static async Task<List<TOutput>> ConvertAll<TInput, TOutput>(this Task<IEnumerable<TInput>> source, Func<TInput, TOutput> mapper) => (await source).ConvertAll(mapper);

		public static async Task<List<TOutput>> ConvertAll<TInput, TOutput>(this Task<ICollection<TInput>> source, Func<TInput, TOutput> mapper) => (await source).ConvertAll(mapper);

		public static async Task<List<TOutput>> ConvertAll<TInput, TOutput>(this Task<IList<TInput>> source, Func<TInput, TOutput> mapper) => (await source).ConvertAll(mapper);

		public static async Task<List<TOutput>> ConvertAll<TInput, TOutput>(this Task<List<TInput>> source, Func<TInput, TOutput> mapper) => (await source).ConvertAll(mapper);

		public static IEnumerable<List<T>> ByBunchOf<T>(this IEnumerable<T> source, int bunchMaxSize)
		{
			using (IEnumerator<T> enumerator = source.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					List<T> bunch = new List<T>(bunchMaxSize);
					for (int i = 0 ; i < bunchMaxSize ; i++)
					{
						bunch.Add(enumerator.Current);

						if (!enumerator.MoveNext())
						{
							break;
						}
					}

					yield return bunch;
				}
			}
		}

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
}