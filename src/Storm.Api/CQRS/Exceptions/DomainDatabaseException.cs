using System.Net;

namespace Storm.Api.CQRS.Exceptions;

/// <summary>
/// Database level failure that should surface as an HTTP error (503 by default).
/// Use for cases like primary unavailable on a SQL Server Always-On setup.
/// </summary>
public class DomainDatabaseException : DomainHttpCodeException
{
	public const string PrimaryUnavailableErrorCode = "DATABASE_PRIMARY_UNAVAILABLE";

	public DomainDatabaseException()
		: base(HttpStatusCode.ServiceUnavailable)
	{
	}

	public DomainDatabaseException(string errorMessage)
		: base(HttpStatusCode.ServiceUnavailable, errorMessage)
	{
	}

	public DomainDatabaseException(string errorCode, string errorMessage)
		: base(HttpStatusCode.ServiceUnavailable, errorCode, errorMessage)
	{
	}

	public DomainDatabaseException(string errorCode, string errorMessage, Exception inner)
		: base(HttpStatusCode.ServiceUnavailable, errorCode, errorMessage, inner)
	{
	}

	public DomainDatabaseException(HttpStatusCode code, string errorCode, string errorMessage)
		: base(code, errorCode, errorMessage)
	{
	}

	public DomainDatabaseException(HttpStatusCode code, string errorCode, string errorMessage, Exception inner)
		: base(code, errorCode, errorMessage, inner)
	{
	}

	public static DomainDatabaseException PrimaryUnavailable(string? reason = null)
		=> new(PrimaryUnavailableErrorCode, reason ?? "Database primary replica is unavailable.");
}
