using System.Data;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Storm.Api.Core.Extensions;

public static class DatabaseMultiExtensions
{
	public static Task<List<TDest>> AsSelectMultiAsync<TTable1, TTable2, TDest>(this SqlExpression<TTable1> sql, IDbConnection connection, Func<TTable1, TTable2, TDest> mapper) =>
		connection.SelectMultiAsync<TTable1, TTable2>(sql)
			.ConvertAll(item => mapper(item.Item1, item.Item2));

	public static Task<List<TDest>> AsSelectMultiAsync<TTable1, TTable2, TTable3, TDest>(this SqlExpression<TTable1> sql, IDbConnection connection, Func<TTable1, TTable2, TTable3, TDest> mapper) =>
		connection.SelectMultiAsync<TTable1, TTable2, TTable3>(sql)
			.ConvertAll(item => mapper(item.Item1, item.Item2, item.Item3));

	public static Task<List<TDest>> AsSelectMultiAsync<TTable1, TTable2, TTable3, TTable4, TDest>(this SqlExpression<TTable1> sql, IDbConnection connection, Func<TTable1, TTable2, TTable3, TTable4, TDest> mapper) =>
		connection.SelectMultiAsync<TTable1, TTable2, TTable3, TTable4>(sql)
			.ConvertAll(item => mapper(item.Item1, item.Item2, item.Item3, item.Item4));

	public static Task<List<TDest>> AsSelectMultiAsync<TTable1, TTable2, TTable3, TTable4, TTable5, TDest>(this SqlExpression<TTable1> sql, IDbConnection connection, Func<TTable1, TTable2, TTable3, TTable4, TTable5, TDest> mapper) =>
		connection.SelectMultiAsync<TTable1, TTable2, TTable3, TTable4, TTable5>(sql)
			.ConvertAll(item => mapper(item.Item1, item.Item2, item.Item3, item.Item4, item.Item5));

	public static Task<List<TDest>> AsSelectMultiAsync<TTable1, TTable2, TTable3, TTable4, TTable5, TTable6, TDest>(this SqlExpression<TTable1> sql, IDbConnection connection, Func<TTable1, TTable2, TTable3, TTable4, TTable5, TTable6, TDest> mapper) =>
		connection.SelectMultiAsync<TTable1, TTable2, TTable3, TTable4, TTable5, TTable6>(sql)
			.ConvertAll(item => mapper(item.Item1, item.Item2, item.Item3, item.Item4, item.Item5, item.Item6));

	public static Task<List<TDest>> AsSelectMultiAsync<TTable1, TTable2, TTable3, TTable4, TTable5, TTable6, TTable7, TDest>(this SqlExpression<TTable1> sql, IDbConnection connection, Func<TTable1, TTable2, TTable3, TTable4, TTable5, TTable6, TTable7, TDest> mapper) =>
		connection.SelectMultiAsync<TTable1, TTable2, TTable3, TTable4, TTable5, TTable6, TTable7>(sql)
			.ConvertAll(item => mapper(item.Item1, item.Item2, item.Item3, item.Item4, item.Item5, item.Item6, item.Item7));

	public static async Task<TDest?> AsSingleMultiAsync<TTable1, TTable2, TDest>(this SqlExpression<TTable1> sql, IDbConnection connection, Func<TTable1, TTable2, TDest> mapper)
	{
		List<Tuple<TTable1, TTable2>>? items = await connection.SelectMultiAsync<TTable1, TTable2>(sql.Limit(1));
		if (items is not { Count: > 0 })
		{
			return default;
		}

		(TTable1? item1, TTable2? item2) = items[0];
		return mapper(item1, item2);
	}

	public static async Task<TDest?> AsSingleMultiAsync<TTable1, TTable2, TTable3, TDest>(this SqlExpression<TTable1> sql, IDbConnection connection, Func<TTable1, TTable2, TTable3, TDest> mapper)
	{
		List<Tuple<TTable1, TTable2, TTable3>>? items = await connection.SelectMultiAsync<TTable1, TTable2, TTable3>(sql.Limit(1));
		if (items is not { Count: > 0 })
		{
			return default;
		}

		(TTable1? item1, TTable2? item2, TTable3? item3) = items[0];
		return mapper(item1, item2, item3);
	}

	public static async Task<TDest?> AsSingleMultiAsync<TTable1, TTable2, TTable3, TTable4, TDest>(this SqlExpression<TTable1> sql, IDbConnection connection, Func<TTable1, TTable2, TTable3, TTable4, TDest> mapper)
	{
		List<Tuple<TTable1, TTable2, TTable3, TTable4>>? items = await connection.SelectMultiAsync<TTable1, TTable2, TTable3, TTable4>(sql.Limit(1));
		if (items is not { Count: > 0 })
		{
			return default;
		}

		(TTable1? item1, TTable2? item2, TTable3? item3, TTable4? item4) = items[0];
		return mapper(item1, item2, item3, item4);
	}

	public static async Task<TDest?> AsSingleMultiAsync<TTable1, TTable2, TTable3, TTable4, TTable5, TDest>(this SqlExpression<TTable1> sql, IDbConnection connection, Func<TTable1, TTable2, TTable3, TTable4, TTable5, TDest> mapper)
	{
		List<Tuple<TTable1, TTable2, TTable3, TTable4, TTable5>>? items = await connection.SelectMultiAsync<TTable1, TTable2, TTable3, TTable4, TTable5>(sql.Limit(1));
		if (items is not { Count: > 0 })
		{
			return default;
		}

		(TTable1? item1, TTable2? item2, TTable3? item3, TTable4? item4, TTable5? item5) = items[0];
		return mapper(item1, item2, item3, item4, item5);
	}

	public static async Task<TDest?> AsSingleMultiAsync<TTable1, TTable2, TTable3, TTable4, TTable5, TTable6, TDest>(this SqlExpression<TTable1> sql, IDbConnection connection, Func<TTable1, TTable2, TTable3, TTable4, TTable5, TTable6, TDest> mapper)
	{
		List<Tuple<TTable1, TTable2, TTable3, TTable4, TTable5, TTable6>>? items = await connection.SelectMultiAsync<TTable1, TTable2, TTable3, TTable4, TTable5, TTable6>(sql.Limit(1));
		if (items is not { Count: > 0 })
		{
			return default;
		}

		(TTable1? item1, TTable2? item2, TTable3? item3, TTable4? item4, TTable5? item5, TTable6? item6) = items[0];
		return mapper(item1, item2, item3, item4, item5, item6);
	}

	public static async Task<TDest?> AsSingleMultiAsync<TTable1, TTable2, TTable3, TTable4, TTable5, TTable6, TTable7, TDest>(this SqlExpression<TTable1> sql, IDbConnection connection, Func<TTable1, TTable2, TTable3, TTable4, TTable5, TTable6, TTable7, TDest> mapper)
	{
		List<Tuple<TTable1, TTable2, TTable3, TTable4, TTable5, TTable6, TTable7>>? items = await connection.SelectMultiAsync<TTable1, TTable2, TTable3, TTable4, TTable5, TTable6, TTable7>(sql.Limit(1));
		if (items is not { Count: > 0 })
		{
			return default;
		}

		(TTable1? item1, TTable2? item2, TTable3? item3, TTable4? item4, TTable5? item5, TTable6? item6, TTable7? item7) = items[0];
		return mapper(item1, item2, item3, item4, item5, item6, item7);
	}

	public static async Task<List<TResult>> AsQueryAsync<T1, T2, TResult>(this string sql, IDbConnection connection, Func<T1, T2, TResult> map, Dictionary<string, object>? parameters = null)
		=> (await connection.QueryAsync(sql, map, parameters)).ToList();

	public static async Task<List<TResult>> AsQueryAsync<T1, T2, T3, TResult>(this string sql, IDbConnection connection, Func<T1, T2, T3, TResult> map, Dictionary<string, object>? parameters = null)
		=> (await connection.QueryAsync(sql, map, parameters)).ToList();

	public static async Task<List<TResult>> AsQueryAsync<T1, T2, T3, T4, TResult>(this string sql, IDbConnection connection, Func<T1, T2, T3, T4, TResult> map, Dictionary<string, object>? parameters = null)
		=> (await connection.QueryAsync(sql, map, parameters)).ToList();

	public static async Task<List<TResult>> AsQueryAsync<T1, T2, T3, T4, T5, TResult>(this string sql, IDbConnection connection, Func<T1, T2, T3, T4, T5, TResult> map, Dictionary<string, object>? parameters = null)
		=> (await connection.QueryAsync(sql, map, parameters)).ToList();

	public static async Task<List<TResult>> AsQueryAsync<T1, T2, T3, T4, T5, T6, TResult>(this string sql, IDbConnection connection, Func<T1, T2, T3, T4, T5, T6, TResult> map, Dictionary<string, object>? parameters = null)
		=> (await connection.QueryAsync(sql, map, parameters)).ToList();

	public static async Task<List<TResult>> AsQueryAsync<T1, T2, T3, T4, T5, T6, T7, TResult>(this string sql, IDbConnection connection, Func<T1, T2, T3, T4, T5, T6, T7, TResult> map, Dictionary<string, object>? parameters = null)
		=> (await connection.QueryAsync(sql, map, parameters)).ToList();

	public static async Task<TResult?> AsQuerySingleAsync<T1, T2, TResult>(this string sql, IDbConnection connection, Func<T1, T2, TResult> map, Dictionary<string, object>? parameters = null)
		=> (await connection.QueryAsync(sql, map, parameters)).FirstOrDefault();

	public static async Task<TResult?> AsQuerySingleAsync<T1, T2, T3, TResult>(this string sql, IDbConnection connection, Func<T1, T2, T3, TResult> map, Dictionary<string, object>? parameters = null)
		=> (await connection.QueryAsync(sql, map, parameters)).FirstOrDefault();

	public static async Task<TResult?> AsQuerySingleAsync<T1, T2, T3, T4, TResult>(this string sql, IDbConnection connection, Func<T1, T2, T3, T4, TResult> map, Dictionary<string, object>? parameters = null)
		=> (await connection.QueryAsync(sql, map, parameters)).FirstOrDefault();

	public static async Task<TResult?> AsQuerySingleAsync<T1, T2, T3, T4, T5, TResult>(this string sql, IDbConnection connection, Func<T1, T2, T3, T4, T5, TResult> map, Dictionary<string, object>? parameters = null)
		=> (await connection.QueryAsync(sql, map, parameters)).FirstOrDefault();

	public static async Task<TResult?> AsQuerySingleAsync<T1, T2, T3, T4, T5, T6, TResult>(this string sql, IDbConnection connection, Func<T1, T2, T3, T4, T5, T6, TResult> map, Dictionary<string, object>? parameters = null)
		=> (await connection.QueryAsync(sql, map, parameters)).FirstOrDefault();

	public static async Task<TResult?> AsQuerySingleAsync<T1, T2, T3, T4, T5, T6, T7, TResult>(this string sql, IDbConnection connection, Func<T1, T2, T3, T4, T5, T6, T7, TResult> map, Dictionary<string, object>? parameters = null)
		=> (await connection.QueryAsync(sql, map, parameters)).FirstOrDefault();
}