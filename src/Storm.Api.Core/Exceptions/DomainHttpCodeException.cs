using System;
using System.Net;

namespace Storm.Api.Core.Exceptions
{
	public class DomainHttpCodeException : DomainException
	{
		public int Code { get; }

		public DomainHttpCodeException(HttpStatusCode code) : base(code.ToString(), string.Empty)
		{
			Code = (int)code;
		}

		public DomainHttpCodeException(HttpStatusCode code, string errorMessage) : base(code.ToString(), errorMessage)
		{
			Code = (int)code;
		}

		public DomainHttpCodeException(HttpStatusCode code, string errorCode, string errorMessage) : base(errorCode, errorMessage)
		{
			Code = (int)code;
		}

		public DomainHttpCodeException(HttpStatusCode code, string errorMessage, Exception inner) : base(code.ToString(), errorMessage, inner)
		{
			Code = (int)code;
		}

		public DomainHttpCodeException(HttpStatusCode code, string errorCode, string errorMessage, Exception inner) : base(errorCode, errorMessage, inner)
		{
			Code = (int)code;
		}
	}
}