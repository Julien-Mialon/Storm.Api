using System.Data;
using Storm.Api.Core.Models;

namespace Storm.Api.Core.Extensions;

public static class ConnectionExtensions
{
	public static Func<T1?, T2?, TDest> Mapper<T1, T2, TDest>(this IDbConnection _, Func<T1?, T2?, TDest> mapper)
		where T1 : ICommonEntity
		where T2 : ICommonEntity
	{
		return InternalMapper;

		TDest InternalMapper(T1? item1, T2? item2)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);

			return mapper(item1, item2);
		}
	}

	public static Func<T1?, T2?, T3?, TDest> Mapper<T1, T2, T3, TDest>(this IDbConnection _, Func<T1?, T2?, T3?, TDest> mapper)
		where T1 : ICommonEntity
		where T2 : ICommonEntity
		where T3 : ICommonEntity
	{
		return InternalMapper;

		TDest InternalMapper(T1? item1, T2? item2, T3? item3)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);
			AssignNullIfNeeded(ref item3);

			return mapper(item1, item2, item3);
		}
	}

	public static Func<T1?, T2?, T3?, T4?, TDest> Mapper<T1, T2, T3, T4, TDest>(this IDbConnection _, Func<T1?, T2?, T3?, T4?, TDest> mapper)
		where T1 : ICommonEntity
		where T2 : ICommonEntity
		where T3 : ICommonEntity
		where T4 : ICommonEntity
	{
		return InternalMapper;

		TDest InternalMapper(T1? item1, T2? item2, T3? item3, T4? item4)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);
			AssignNullIfNeeded(ref item3);
			AssignNullIfNeeded(ref item4);

			return mapper(item1, item2, item3, item4);
		}
	}

	public static Func<T1?, T2?, T3?, T4?, T5?, TDest> Mapper<T1, T2, T3, T4, T5, TDest>(this IDbConnection _, Func<T1?, T2?, T3?, T4?, T5?, TDest> mapper)
		where T1 : ICommonEntity
		where T2 : ICommonEntity
		where T3 : ICommonEntity
		where T4 : ICommonEntity
		where T5 : ICommonEntity
	{
		return InternalMapper;

		TDest InternalMapper(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);
			AssignNullIfNeeded(ref item3);
			AssignNullIfNeeded(ref item4);
			AssignNullIfNeeded(ref item5);

			return mapper(item1, item2, item3, item4, item5);
		}
	}

	public static Func<T1?, T2?, T3?, T4?, T5?, T6?, TDest> Mapper<T1, T2, T3, T4, T5, T6, TDest>(this IDbConnection _, Func<T1?, T2?, T3?, T4?, T5?, T6?, TDest> mapper)
		where T1 : ICommonEntity
		where T2 : ICommonEntity
		where T3 : ICommonEntity
		where T4 : ICommonEntity
		where T5 : ICommonEntity
		where T6 : ICommonEntity
	{
		return InternalMapper;

		TDest InternalMapper(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);
			AssignNullIfNeeded(ref item3);
			AssignNullIfNeeded(ref item4);
			AssignNullIfNeeded(ref item5);
			AssignNullIfNeeded(ref item6);

			return mapper(item1, item2, item3, item4, item5, item6);
		}
	}

	public static Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TDest> Mapper<T1, T2, T3, T4, T5, T6, T7, TDest>(this IDbConnection _, Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TDest> mapper)
		where T1 : ICommonEntity
		where T2 : ICommonEntity
		where T3 : ICommonEntity
		where T4 : ICommonEntity
		where T5 : ICommonEntity
		where T6 : ICommonEntity
		where T7 : ICommonEntity
	{
		return InternalMapper;

		TDest InternalMapper(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6, T7? item7)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);
			AssignNullIfNeeded(ref item3);
			AssignNullIfNeeded(ref item4);
			AssignNullIfNeeded(ref item5);
			AssignNullIfNeeded(ref item6);
			AssignNullIfNeeded(ref item7);

			return mapper(item1, item2, item3, item4, item5, item6, item7);
		}
	}

	private static void AssignNullIfNeeded<T>(ref T? entity)
		where T : ICommonEntity
	{
		if (entity.IsNullOrDefault())
		{
			entity = default;
		}
	}
}