using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using Storm.Api.Core.Logs;

namespace Storm.Api.Logs.Appenders
{
	public class RequestHeaderAppender : ILogAppender
	{
		private readonly IActionContextAccessor _actionContextAccessor;

		public RequestHeaderAppender(IActionContextAccessor actionContextAccessor)
		{
			_actionContextAccessor = actionContextAccessor;
		}

		public void Append(IObjectWriter logEntry)
		{
			HttpContext httpContext = _actionContextAccessor.ActionContext?.HttpContext;
			if (httpContext == null)
			{
				return;
			}

			DumpRequestHeader(httpContext, logEntry);
		}

		public static void DumpRequestHeader(HttpContext httpContext, IObjectWriter logEntry)
		{
			logEntry.WriteObject("Headers", x =>
			{
				foreach (KeyValuePair<string,StringValues> value in httpContext.Request.Headers)
				{
					x.WriteProperty(value.Key, value.Value.ToString());
				}
			});
		}
	}
}