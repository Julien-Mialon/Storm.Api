using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Storm.Api.Swaggers.Attributes
{
	public class ResponseAttribute : ProducesResponseTypeAttribute
	{
		public ResponseAttribute(Type type, HttpStatusCode statusCode) : base(type, (int)statusCode)
		{
		}
	}
}