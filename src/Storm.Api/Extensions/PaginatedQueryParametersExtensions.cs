using Storm.Api.Core.Domains.Parameters;
using Storm.Api.Dtos;

namespace Storm.Api.Extensions
{
	public static class PaginatedQueryParametersExtensions
	{
		public static PaginationParameter ToPaginationParameter(this PaginatedQueryParameters source, int defaultPage = 0, int defaultCount = 25)
		{
			return new PaginationParameter
			{
				Page = source?.Page ?? defaultPage,
				Count = source?.Count ?? defaultCount,
			};
		}
	}
}