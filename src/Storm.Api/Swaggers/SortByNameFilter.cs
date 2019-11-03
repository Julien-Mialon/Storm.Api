using System.Linq;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Storm.Api.Swaggers
{
	public class SortByNameFilter : IDocumentFilter
	{
		public void Apply(Swashbuckle.AspNetCore.Swagger.SwaggerDocument swaggerDoc, DocumentFilterContext context)
		{
			swaggerDoc.Paths = swaggerDoc.Paths.OrderBy(x => x.Key).ToList().ToDictionary(e => e.Key, e => e.Value);
		}
	}
}