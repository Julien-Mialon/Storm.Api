using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Storm.Api.Core.Logs;

namespace Storm.Api.Logs.Appenders
{
	public class RequestContextAppender : ILogAppender
	{
		private readonly IActionContextAccessor _actionContextAccessor;

		public RequestContextAppender(IActionContextAccessor actionContextAccessor)
		{
			_actionContextAccessor = actionContextAccessor;
		}

		public void Append(IObjectWriter logEntry)
		{
			ActionContext actionContext = _actionContextAccessor.ActionContext;
			if (actionContext == null)
			{
				return;
			}

			DumpRequestContext(actionContext.HttpContext, actionContext.ActionDescriptor.RouteValues, logEntry);
		}

		public static void DumpRequestContext(HttpContext httpContext, IDictionary<string, string> routeValues, IObjectWriter logEntry)
		{
			if (!(httpContext.Items.TryGetValue(nameof(RequestContextAppender), out object rawRequestId) && rawRequestId is Guid requestId))
			{
				httpContext.Items[nameof(RequestContextAppender)] = requestId = Guid.NewGuid();
			}

			logEntry.WriteObject("Request", x =>
			{
				x.WriteProperty("Id", requestId.ToString())
					.WriteProperty("Path", httpContext.Request.Path)
					.WriteProperty("Method", httpContext.Request.Method);
				if (routeValues != null)
				{
					x.WriteObject("routeValues", y =>
					{
						foreach (KeyValuePair<string, string> value in routeValues)
						{
							y.WriteProperty(value.Key, value.Value);
						}
					});
				}
			});
		}
	}
}