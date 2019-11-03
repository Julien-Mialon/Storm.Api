using Swashbuckle.AspNetCore.Annotations;

namespace Storm.Api.Swaggers.Attributes
{
	public class CategoryAttribute : SwaggerOperationAttribute
	{
		public CategoryAttribute(string category) : base(category)
		{
			Tags = new[] { category };
		}
	}
}