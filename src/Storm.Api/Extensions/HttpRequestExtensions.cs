using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Storm.Api.Core.Validations;

namespace Storm.Api.Extensions
{
	public static class HttpRequestExtensions
	{
		public static bool TryGetHeaderOrQueryParameter(this IHttpContextAccessor contextAccessor, string headerName, string queryStringParameterName, out string value) =>
			contextAccessor.HttpContext.Request.TryGetHeaderOrQueryParameter(headerName, queryStringParameterName, out value);

		public static bool TryGetHeader(this IHttpContextAccessor contextAccessor, string headerName, out string value) =>
			contextAccessor.HttpContext.Request.TryGetHeader(headerName, out value);

		public static bool TryGetQueryParameter(this IHttpContextAccessor contextAccessor, string queryStringParameterName, out string value) =>
			contextAccessor.HttpContext.Request.TryGetQueryParameter(queryStringParameterName, out value);

		public static bool TryGetHeaderOrQueryParameter(this HttpRequest request, string headerName, string queryStringParameterName, out string value)
		{
			if (request.TryGetHeader(headerName, out value))
			{
				return true;
			}

			if (request.TryGetQueryParameter(queryStringParameterName, out value))
			{
				return true;
			}

			value = default;
			return false;
		}

		public static bool TryGetHeader(this HttpRequest request, string headerName, out string value)
		{
			if (request.Headers.TryGetValue(headerName, out var values))
			{
				value = values.ToString().Trim();
				if (value.IsNotNullOrEmpty())
				{
					return true;
				}
			}

			value = default;
			return false;
		}

		public static bool TryGetQueryParameter(this HttpRequest request, string queryParameterName, out string value)
		{
			if (request.Query.TryGetValue(queryParameterName, out var values))
			{
				value = values.ToString().Trim();
				if (value.IsNotNullOrEmpty())
				{
					return true;
				}
			}

			value = default;
			return false;
		}

		public static CultureInfo RequestCulture(this IHttpContextAccessor contextAccessor) =>
			contextAccessor.HttpContext.Request.RequestCulture();

		public static CultureInfo RequestCulture(this HttpRequest request)
		{
			var feature = request.HttpContext.Features.Get<IRequestCultureFeature>();
			return feature.RequestCulture.Culture;
		}
	}
}