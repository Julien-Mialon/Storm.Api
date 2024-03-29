using System.Net;
using Storm.Api.Core.Exceptions;

namespace Storm.Api.Core.Extensions;

public static class ExceptionExtensions
{
	public static void DomainHttpCodeExceptionIfNull(this object o, HttpStatusCode statusCode, string errorCode = "", string errorMessage = "")
	{
		if (o is null)
		{
			throw new DomainHttpCodeException(statusCode, errorCode, errorMessage);
		}
	}

	public static void UnauthorizedIfNull(this object o, string errorCode = "", string errorMessage = "")
		=> o.DomainHttpCodeExceptionIfNull(HttpStatusCode.Unauthorized, errorCode, errorMessage);

	public static void ForbiddenIfNull(this object o, string errorCode = "", string errorMessage = "")
		=> o.DomainHttpCodeExceptionIfNull(HttpStatusCode.Forbidden, errorCode, errorMessage);

	public static void NotFoundIfNull(this object o, string errorCode = "", string errorMessage = "")
		=> o.DomainHttpCodeExceptionIfNull(HttpStatusCode.NotFound, errorCode, errorMessage);
}