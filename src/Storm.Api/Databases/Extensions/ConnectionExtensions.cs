using System.Data;
using Storm.Api.Databases.Models;

namespace Storm.Api.Databases.Extensions;

public static class ConnectionExtensions
{
	public static Func<T1?, T2?, TDest> Mapper<T1, T2, TDest>(this IDbConnection _, Func<T1?, T2?, TDest> mapper)
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
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
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
		where T3 : IDateTrackingEntity
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
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
		where T3 : IDateTrackingEntity
		where T4 : IDateTrackingEntity
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
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
		where T3 : IDateTrackingEntity
		where T4 : IDateTrackingEntity
		where T5 : IDateTrackingEntity
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
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
		where T3 : IDateTrackingEntity
		where T4 : IDateTrackingEntity
		where T5 : IDateTrackingEntity
		where T6 : IDateTrackingEntity
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
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
		where T3 : IDateTrackingEntity
		where T4 : IDateTrackingEntity
		where T5 : IDateTrackingEntity
		where T6 : IDateTrackingEntity
		where T7 : IDateTrackingEntity
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

	public static Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, TDest> Mapper<T1, T2, T3, T4, T5, T6, T7, T8, TDest>(this IDbConnection _, Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, TDest> mapper)
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
		where T3 : IDateTrackingEntity
		where T4 : IDateTrackingEntity
		where T5 : IDateTrackingEntity
		where T6 : IDateTrackingEntity
		where T7 : IDateTrackingEntity
		where T8 : IDateTrackingEntity
	{
		return InternalMapper;

		TDest InternalMapper(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6, T7? item7, T8? item8)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);
			AssignNullIfNeeded(ref item3);
			AssignNullIfNeeded(ref item4);
			AssignNullIfNeeded(ref item5);
			AssignNullIfNeeded(ref item6);
			AssignNullIfNeeded(ref item7);
			AssignNullIfNeeded(ref item8);

			return mapper(item1, item2, item3, item4, item5, item6, item7, item8);
		}
	}

	public static Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, TDest> Mapper<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDest>(this IDbConnection _, Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, TDest> mapper)
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
		where T3 : IDateTrackingEntity
		where T4 : IDateTrackingEntity
		where T5 : IDateTrackingEntity
		where T6 : IDateTrackingEntity
		where T7 : IDateTrackingEntity
		where T8 : IDateTrackingEntity
		where T9 : IDateTrackingEntity
	{
		return InternalMapper;

		TDest InternalMapper(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6, T7? item7, T8? item8, T9? item9)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);
			AssignNullIfNeeded(ref item3);
			AssignNullIfNeeded(ref item4);
			AssignNullIfNeeded(ref item5);
			AssignNullIfNeeded(ref item6);
			AssignNullIfNeeded(ref item7);
			AssignNullIfNeeded(ref item8);
			AssignNullIfNeeded(ref item9);

			return mapper(item1, item2, item3, item4, item5, item6, item7, item8, item9);
		}
	}

	public static Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, TDest> Mapper<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDest>(this IDbConnection _, Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, TDest> mapper)
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
		where T3 : IDateTrackingEntity
		where T4 : IDateTrackingEntity
		where T5 : IDateTrackingEntity
		where T6 : IDateTrackingEntity
		where T7 : IDateTrackingEntity
		where T8 : IDateTrackingEntity
		where T9 : IDateTrackingEntity
		where T10 : IDateTrackingEntity
	{
		return InternalMapper;

		TDest InternalMapper(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6, T7? item7, T8? item8, T9? item9, T10? item10)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);
			AssignNullIfNeeded(ref item3);
			AssignNullIfNeeded(ref item4);
			AssignNullIfNeeded(ref item5);
			AssignNullIfNeeded(ref item6);
			AssignNullIfNeeded(ref item7);
			AssignNullIfNeeded(ref item8);
			AssignNullIfNeeded(ref item9);
			AssignNullIfNeeded(ref item10);

			return mapper(item1, item2, item3, item4, item5, item6, item7, item8, item9, item10);
		}
	}

	public static Func<T1?, T2?, (T1? item1, T2? item2)> TupleMapper<T1, T2>(this IDbConnection _)
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
	{
		return InternalMapper;

		(T1? item1, T2? item2) InternalMapper(T1? item1, T2? item2)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);

			return (item1, item2);
		}
	}

	public static Func<T1?, T2?, T3?, (T1? item1, T2? item2, T3? item3)> TupleMapper<T1, T2, T3>(this IDbConnection _)
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
		where T3 : IDateTrackingEntity
	{
		return InternalMapper;

		(T1? item1, T2? item2, T3? item3) InternalMapper(T1? item1, T2? item2, T3? item3)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);
			AssignNullIfNeeded(ref item3);

			return (item1, item2, item3);
		}
	}

	public static Func<T1?, T2?, T3?, T4?, (T1? item1, T2? item2, T3? item3, T4? item4)> TupleMapper<T1, T2, T3, T4>(this IDbConnection _)
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
		where T3 : IDateTrackingEntity
		where T4 : IDateTrackingEntity
	{
		return InternalMapper;

		(T1? item1, T2? item2, T3? item3, T4? item4) InternalMapper(T1? item1, T2? item2, T3? item3, T4? item4)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);
			AssignNullIfNeeded(ref item3);
			AssignNullIfNeeded(ref item4);

			return (item1, item2, item3, item4);
		}
	}

	public static Func<T1?, T2?, T3?, T4?, T5?, (T1? item1, T2? item2, T3? item3, T4? item4, T5? item5)> TupleMapper<T1, T2, T3, T4, T5>(this IDbConnection _)
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
		where T3 : IDateTrackingEntity
		where T4 : IDateTrackingEntity
		where T5 : IDateTrackingEntity
	{
		return InternalMapper;

		(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5) InternalMapper(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);
			AssignNullIfNeeded(ref item3);
			AssignNullIfNeeded(ref item4);
			AssignNullIfNeeded(ref item5);

			return (item1, item2, item3, item4, item5);
		}
	}

	public static Func<T1?, T2?, T3?, T4?, T5?, T6?, (T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6)> TupleMapper<T1, T2, T3, T4, T5, T6>(this IDbConnection _)
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
		where T3 : IDateTrackingEntity
		where T4 : IDateTrackingEntity
		where T5 : IDateTrackingEntity
		where T6 : IDateTrackingEntity
	{
		return InternalMapper;

		(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6) InternalMapper(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);
			AssignNullIfNeeded(ref item3);
			AssignNullIfNeeded(ref item4);
			AssignNullIfNeeded(ref item5);
			AssignNullIfNeeded(ref item6);

			return (item1, item2, item3, item4, item5, item6);
		}
	}

	public static Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, (T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6, T7? item7)> TupleMapper<T1, T2, T3, T4, T5, T6, T7>(this IDbConnection _)
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
		where T3 : IDateTrackingEntity
		where T4 : IDateTrackingEntity
		where T5 : IDateTrackingEntity
		where T6 : IDateTrackingEntity
		where T7 : IDateTrackingEntity
	{
		return InternalMapper;

		(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6, T7? item7) InternalMapper(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6, T7? item7)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);
			AssignNullIfNeeded(ref item3);
			AssignNullIfNeeded(ref item4);
			AssignNullIfNeeded(ref item5);
			AssignNullIfNeeded(ref item6);
			AssignNullIfNeeded(ref item7);

			return (item1, item2, item3, item4, item5, item6, item7);
		}
	}

	public static Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, (T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6, T7? item7, T8? item8)> TupleMapper<T1, T2, T3, T4, T5, T6, T7, T8>(this IDbConnection _)
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
		where T3 : IDateTrackingEntity
		where T4 : IDateTrackingEntity
		where T5 : IDateTrackingEntity
		where T6 : IDateTrackingEntity
		where T7 : IDateTrackingEntity
		where T8 : IDateTrackingEntity
	{
		return InternalMapper;

		(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6, T7? item7, T8? item8) InternalMapper(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6, T7? item7, T8? item8)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);
			AssignNullIfNeeded(ref item3);
			AssignNullIfNeeded(ref item4);
			AssignNullIfNeeded(ref item5);
			AssignNullIfNeeded(ref item6);
			AssignNullIfNeeded(ref item7);
			AssignNullIfNeeded(ref item8);

			return (item1, item2, item3, item4, item5, item6, item7, item8);
		}
	}

	public static Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, (T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6, T7? item7, T8? item8, T9? item9)> TupleMapper<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this IDbConnection _)
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
		where T3 : IDateTrackingEntity
		where T4 : IDateTrackingEntity
		where T5 : IDateTrackingEntity
		where T6 : IDateTrackingEntity
		where T7 : IDateTrackingEntity
		where T8 : IDateTrackingEntity
		where T9 : IDateTrackingEntity
	{
		return InternalMapper;

		(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6, T7? item7, T8? item8, T9? item9) InternalMapper(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6, T7? item7, T8? item8, T9? item9)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);
			AssignNullIfNeeded(ref item3);
			AssignNullIfNeeded(ref item4);
			AssignNullIfNeeded(ref item5);
			AssignNullIfNeeded(ref item6);
			AssignNullIfNeeded(ref item7);
			AssignNullIfNeeded(ref item8);
			AssignNullIfNeeded(ref item9);

			return (item1, item2, item3, item4, item5, item6, item7, item8, item9);
		}
	}

	public static Func<T1?, T2?, T3?, T4?, T5?, T6?, T7?, T8?, T9?, T10?, (T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6, T7? item7, T8? item8, T9? item9, T10? item10)> TupleMapper<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this IDbConnection _)
		where T1 : IDateTrackingEntity
		where T2 : IDateTrackingEntity
		where T3 : IDateTrackingEntity
		where T4 : IDateTrackingEntity
		where T5 : IDateTrackingEntity
		where T6 : IDateTrackingEntity
		where T7 : IDateTrackingEntity
		where T8 : IDateTrackingEntity
		where T9 : IDateTrackingEntity
		where T10 : IDateTrackingEntity
	{
		return InternalMapper;

		(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6, T7? item7, T8? item8, T9? item9, T10? item10) InternalMapper(T1? item1, T2? item2, T3? item3, T4? item4, T5? item5, T6? item6, T7? item7, T8? item8, T9? item9, T10? item10)
		{
			AssignNullIfNeeded(ref item1);
			AssignNullIfNeeded(ref item2);
			AssignNullIfNeeded(ref item3);
			AssignNullIfNeeded(ref item4);
			AssignNullIfNeeded(ref item5);
			AssignNullIfNeeded(ref item6);
			AssignNullIfNeeded(ref item7);
			AssignNullIfNeeded(ref item8);
			AssignNullIfNeeded(ref item9);
			AssignNullIfNeeded(ref item10);

			return (item1, item2, item3, item4, item5, item6, item7, item8, item9, item10);
		}
	}

	private static void AssignNullIfNeeded<T>(ref T? entity)
		where T : IDateTrackingEntity
	{
		if (entity.IsNullOrDefault())
		{
			entity = default;
		}
	}
}