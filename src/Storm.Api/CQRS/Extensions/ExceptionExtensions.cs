using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using Storm.Api.CQRS.Exceptions;

namespace Storm.Api.CQRS.Extensions;

public static class ExceptionExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T BadRequestIfNull<T>([NotNull] this T? o, string errorCode = "", string errorMessage = "")
		=> DomainHttpCodeExceptionIfNull(o, HttpStatusCode.BadRequest, errorCode, errorMessage);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T BadRequestIfNull<T>([NotNull] this T? o, string errorCode = "", string errorMessage = "") where T : struct
		=> DomainHttpCodeExceptionIfNull(o, HttpStatusCode.BadRequest, errorCode, errorMessage);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T UnauthorizedIfNull<T>([NotNull] this T? o, string errorCode = "", string errorMessage = "")
		=> DomainHttpCodeExceptionIfNull(o, HttpStatusCode.Unauthorized, errorCode, errorMessage);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T UnauthorizedIfNull<T>([NotNull] this T? o, string errorCode = "", string errorMessage = "") where T : struct
		=> DomainHttpCodeExceptionIfNull(o, HttpStatusCode.Unauthorized, errorCode, errorMessage);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T ForbiddenIfNull<T>([NotNull] this T? o, string errorCode = "", string errorMessage = "")
		=> DomainHttpCodeExceptionIfNull(o, HttpStatusCode.Forbidden, errorCode, errorMessage);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T ForbiddenIfNull<T>([NotNull] this T? o, string errorCode = "", string errorMessage = "") where T : struct
		=> DomainHttpCodeExceptionIfNull(o, HttpStatusCode.Forbidden, errorCode, errorMessage);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T NotFoundIfNull<T>([NotNull] this T? o, string errorCode = "", string errorMessage = "")
		=> DomainHttpCodeExceptionIfNull(o, HttpStatusCode.NotFound, errorCode, errorMessage);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T NotFoundIfNull<T>([NotNull] this T? o, string errorCode = "", string errorMessage = "") where T : struct
		=> DomainHttpCodeExceptionIfNull(o, HttpStatusCode.NotFound, errorCode, errorMessage);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Task<T> BadRequestIfNull<T>(this Task<T?> o, string errorCode = "", string errorMessage = "")
		=> DomainHttpCodeExceptionIfNull(o, HttpStatusCode.BadRequest, errorCode, errorMessage);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Task<T> BadRequestIfNull<T>(this Task<T?> o, string errorCode = "", string errorMessage = "") where T : struct
		=> DomainHttpCodeExceptionIfNull(o, HttpStatusCode.BadRequest, errorCode, errorMessage);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Task<T> UnauthorizedIfNull<T>(this Task<T?> o, string errorCode = "", string errorMessage = "")
		=> DomainHttpCodeExceptionIfNull(o, HttpStatusCode.Unauthorized, errorCode, errorMessage);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Task<T> UnauthorizedIfNull<T>(this Task<T?> o, string errorCode = "", string errorMessage = "") where T : struct
		=> DomainHttpCodeExceptionIfNull(o, HttpStatusCode.Unauthorized, errorCode, errorMessage);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Task<T> ForbiddenIfNull<T>(this Task<T?> o, string errorCode = "", string errorMessage = "")
		=> DomainHttpCodeExceptionIfNull(o, HttpStatusCode.Forbidden, errorCode, errorMessage);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Task<T> ForbiddenIfNull<T>(this Task<T?> o, string errorCode = "", string errorMessage = "") where T : struct
		=> DomainHttpCodeExceptionIfNull(o, HttpStatusCode.Forbidden, errorCode, errorMessage);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Task<T> NotFoundIfNull<T>(this Task<T?> o, string errorCode = "", string errorMessage = "")
		=> DomainHttpCodeExceptionIfNull(o, HttpStatusCode.NotFound, errorCode, errorMessage);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Task<T> NotFoundIfNull<T>(this Task<T?> o, string errorCode = "", string errorMessage = "") where T : struct
		=> DomainHttpCodeExceptionIfNull(o, HttpStatusCode.NotFound, errorCode, errorMessage);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T DomainHttpCodeExceptionIfNull<T>([NotNull] this T? o, HttpStatusCode statusCode, string errorCode = "", string errorMessage = "")
	{
		if (o is null)
		{
			throw new DomainHttpCodeException(statusCode, errorCode, errorMessage);
		}

		return o;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T DomainHttpCodeExceptionIfNull<T>([NotNull] this T? o, HttpStatusCode statusCode, string errorCode = "", string errorMessage = "") where T : struct
	{
		if (o is null)
		{
			throw new DomainHttpCodeException(statusCode, errorCode, errorMessage);
		}

		return o.Value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async Task<T> DomainHttpCodeExceptionIfNull<T>(Task<T?> task, HttpStatusCode statusCode, string errorCode = "", string errorMessage = "")
	{
		T? result = await task;
		if (result is null)
		{
			throw new DomainHttpCodeException(statusCode, errorCode, errorMessage);
		}

		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async Task<T> DomainHttpCodeExceptionIfNull<T>(Task<T?> task, HttpStatusCode statusCode, string errorCode = "", string errorMessage = "") where T : struct
	{
		T? result = await task;
		if (result is null)
		{
			throw new DomainHttpCodeException(statusCode, errorCode, errorMessage);
		}

		return result.Value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T DomainExceptionIfNull<T>([NotNull] this T? o, string errorCode, string errorMessage = "")
	{
		if (o is null)
		{
			throw new DomainException(errorCode, errorMessage);
		}

		return o;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T DomainExceptionIfNull<T>([NotNull] this T? o, string errorCode, string errorMessage = "") where T : struct
	{
		if (o is null)
		{
			throw new DomainException(errorCode, errorMessage);
		}

		return o.Value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async Task<T> DomainExceptionIfNull<T>(this Task<T?> o, string errorCode, string errorMessage = "")
	{
		T? result = await o;
		if (result is null)
		{
			throw new DomainException(errorCode, errorMessage);
		}

		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async Task<T> DomainExceptionIfNull<T>(this Task<T?> o, string errorCode, string errorMessage = "") where T : struct
	{
		T? result = await o;
		if (result is null)
		{
			throw new DomainException(errorCode, errorMessage);
		}

		return result.Value;
	}
}