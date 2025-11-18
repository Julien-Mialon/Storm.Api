using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using Storm.Api.Logs.Interfaces;

namespace Storm.Api.Logs.Appenders;

public class RequestContextAppender : ILogAppender
{
	private static readonly ISet<string> HIDDEN_HEADERS = new HashSet<string>
	{
		"X-ApiKey",
		"X-Token",
		"Authorization",
		//TODO: see if we want to hide more headers (either sensitive or useless)
	};

	private readonly IHttpContextAccessor _httpContextAccessor;

	public bool MultipleAllowed => false;

	public RequestContextAppender(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public void Append(IObjectWriter logEntry)
	{
		if (_httpContextAccessor.HttpContext is { } httpContext &&
		    httpContext.GetEndpoint() is { } endpoints &&
		    endpoints.Metadata.GetMetadata<ActionDescriptor>() is { } actionDescriptor
		   )
		{
			DumpRequestContext(httpContext, actionDescriptor.RouteValues, logEntry);
		}
	}

	internal static void DumpRequestContext(HttpContext httpContext, IDictionary<string, string?>? routeValues, IObjectWriter logEntry)
	{
		if (httpContext.Items.TryGetValue(nameof(RequestContextAppender), out object? rawRequestId) is false || rawRequestId is not Guid requestId)
		{
			httpContext.Items[nameof(RequestContextAppender)] = requestId = Guid.NewGuid();
		}

		logEntry.WriteObject("Request", x =>
		{
			x.WriteProperty("Id", requestId.ToString())
				.WriteProperty("Path", httpContext.Request.Path)
				.WriteProperty("Method", httpContext.Request.Method)
				.WriteObject("Headers", y =>
				{
					foreach (KeyValuePair<string, StringValues> value in httpContext.Request.Headers)
					{
						if (HIDDEN_HEADERS.Contains(value.Key))
						{
							continue;
						}

						y.WriteProperty(value.Key, value.Value.ToString());
					}
				});

			if (routeValues is { Count: > 0 })
			{
				x.WriteObject("RouteValues", y =>
				{
					foreach (KeyValuePair<string, string?> value in routeValues)
					{
						y.WriteProperty(value.Key, value.Value);
					}
				});
			}
		});
	}
}