using System.Data;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Storm.Api.Databases.Models;

namespace Storm.Api.Databases.Extensions;

public static class DatabaseExtensions
{
	public static Task<List<T>> AsSelectAsync<T>(this SqlExpression<T> expression, IDbConnection connection)
	{
		return connection.SelectAsync(expression);
	}

	public static Task<List<T>> AsSelectIntoAsync<T>(this ISqlExpression expression, IDbConnection connection)
	{
		return connection.SelectAsync<T>(expression);
	}

	public static async Task<T?> AsSingleAsync<T>(this SqlExpression<T> expression, IDbConnection connection)
	{
		return await connection.SingleAsync(expression);
	}

	public static Task<T?> AsSingleIntoAsync<T>(this ISqlExpression expression, IDbConnection connection)
	{
		return connection.SingleAsync<T?>(expression);
	}

	public static Task<Dictionary<TKey, TValue>> AsDictionaryAsync<TKey, TValue>(this ISqlExpression expression, IDbConnection connection, CancellationToken ct = default) where TKey : notnull
	{
		return connection.DictionaryAsync<TKey, TValue>(expression, ct);
	}

	public static Task<Dictionary<TKey, List<TValue>>> AsLookupAsync<TKey, TValue>(this ISqlExpression expression, IDbConnection connection, CancellationToken ct = default) where TKey : notnull
	{
		return connection.LookupAsync<TKey, TValue>(expression, ct);
	}

	public static async Task<int> AsCountAsync<T>(this SqlExpression<T> expression, IDbConnection connection)
	{
		return (int)await connection.CountAsync(expression);
	}

	public static async Task<List<T>> AsColumnAsync<T>(this ISqlExpression expression, IDbConnection connection)
	{
		return await connection.ColumnAsync<T>(expression);
	}

	public static Task<HashSet<T>> AsColumnDistinctAsync<T>(this ISqlExpression expression, IDbConnection connection, CancellationToken ct = default)
	{
		return connection.ColumnDistinctAsync<T>(expression, ct);
	}

	public static Task<bool> AsExistsAsync<T>(this SqlExpression<T> expression, IDbConnection connection)
	{
		return connection.ExistsAsync(expression);
	}

	public static Task<T> AsScalarAsync<T>(this ISqlExpression expression, IDbConnection connection, CancellationToken ct = default)
	{
		return connection.ScalarAsync<T>(expression, ct);
	}

	public static string TableName(this Type type)
	{
		return type.TableRef().GetTableName();
	}

	public static TableRef TableRef(this Type type)
	{
		return new(type);
	}

	public static string SqlLimitString(this IDbConnection connection, int? offset = null, int? count = null)
	{
		return connection.GetDialectProvider().SqlLimit(offset, count);
	}

	public static async Task<int> SoftDeleteAsync<TEntity>(this IDbConnection connection, Expression<Func<TEntity, bool>> whereClause) where TEntity : IDateTrackingEntity
	{
		return await connection.UpdateAsync(new
		{
			IsDeleted = true,
			EntityDeletedDate = DateTime.UtcNow
		}, whereClause);
	}
}