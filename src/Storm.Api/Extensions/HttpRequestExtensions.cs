using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Primitives;

namespace Storm.Api.Extensions;

public static class HttpRequestExtensions
{
	public static bool TryGetHeaderOrQueryParameter(this IHttpContextAccessor contextAccessor, string headerName, string queryStringParameterName, [NotNullWhen(true)] out string? value)
	{
		if (contextAccessor.HttpContext is { } context)
		{
			return context.Request.TryGetHeaderOrQueryParameter(headerName, queryStringParameterName, out value);
		}

		value = null;
		return false;
	}

	public static bool TryGetHeader(this IHttpContextAccessor contextAccessor, string headerName, [NotNullWhen(true)] out string? value)
	{
		if (contextAccessor.HttpContext is { } context)
		{
			return context.Request.TryGetHeader(headerName, out value);
		}

		value = null;
		return false;
	}

	public static bool TryGetQueryParameter(this IHttpContextAccessor contextAccessor, string queryStringParameterName, [NotNullWhen(true)] out string? value)
	{
		if (contextAccessor.HttpContext is { } context)
		{
			return context.Request.TryGetQueryParameter(queryStringParameterName, out value);
		}

		value = null;
		return false;
	}

	public static bool TryGetHeaderOrQueryParameter(this HttpRequest request, string headerName, string queryStringParameterName, [NotNullWhen(true)] out string? value)
	{
		if (request.TryGetHeader(headerName, out value))
		{
			return true;
		}

		if (request.TryGetQueryParameter(queryStringParameterName, out value))
		{
			return true;
		}

		value = null;
		return false;
	}

	public static bool TryGetHeader(this HttpRequest request, string headerName, [NotNullWhen(true)] out string? value)
	{
		if (request.Headers.TryGetValue(headerName, out StringValues values))
		{
			value = values.ToString().Trim();
			if (value.IsNotNullOrEmpty())
			{
				return true;
			}
		}

		value = null;
		return false;
	}

	public static bool TryGetQueryParameter(this HttpRequest request, string queryParameterName, [NotNullWhen(true)] out string? value)
	{
		if (request.Query.TryGetValue(queryParameterName, out StringValues values))
		{
			value = values.ToString().Trim();
			if (value.IsNotNullOrEmpty())
			{
				return true;
			}
		}

		value = null;
		return false;
	}

	public static CultureInfo RequestCulture(this IHttpContextAccessor contextAccessor)
	{
		if (contextAccessor.HttpContext?.Request is { } request)
		{
			return request.RequestCulture();
		}

		return CultureInfo.InvariantCulture;
	}

	public static CultureInfo RequestCulture(this HttpRequest request)
	{
		IRequestCultureFeature? feature = request.HttpContext.Features.Get<IRequestCultureFeature>();
		return feature?.RequestCulture.Culture ?? CultureInfo.InvariantCulture;
	}
}