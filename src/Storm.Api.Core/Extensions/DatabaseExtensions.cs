using System.Data;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Storm.Api.Core.Models;

namespace Storm.Api.Core.Extensions;

public static class DatabaseExtensions
{
	public static Task<List<T>> AsSelectAsync<T>(this SqlExpression<T> expression, IDbConnection connection)
	{
		return connection.SelectAsync(expression);
	}

	public static Task<T> AsSingleAsync<T>(this SqlExpression<T> expression, IDbConnection connection)
	{
		return connection.SingleAsync(expression);
	}

	public static async Task<int> AsCountAsync<T>(this SqlExpression<T> expression, IDbConnection connection)
	{
		return (int)await connection.CountAsync(expression);
	}

	public static async Task<List<T>> AsColumnAsync<T>(this ISqlExpression expression, IDbConnection connection)
	{
		return await connection.ColumnAsync<T>(expression);
	}

	public static Task<bool> AsExistsAsync<T>(this SqlExpression<T> expression, IDbConnection connection)
	{
		return connection.ExistsAsync(expression);
	}

	public static string TableName(this Type type)
	{
		return type.GetModelMetadata().ModelName;
	}

	public static string SqlLimitString(this IDbConnection connection, int? offset = null, int? count = null)
	{
		return connection.GetDialectProvider().SqlLimit(offset, count);
	}

	public static async Task<int> SoftDeleteAsync<TEntity>(this IDbConnection connection, Expression<Func<TEntity, bool>> whereClause) where TEntity : ICommonEntity
	{
		return await connection.UpdateAsync(new
		{
			IsDeleted = true,
			EntityDeletedDate = DateTime.UtcNow
		}, whereClause);
	}
}