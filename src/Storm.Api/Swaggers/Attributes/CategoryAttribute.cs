using Swashbuckle.AspNetCore.Annotations;

namespace Storm.Api.Swaggers.Attributes
{
	public class CategoryAttribute : SwaggerOperationAttribute
	{
		public string Category { get; init; }

		public CategoryAttribute(string category) : base(category)
		{
			Category = category;
			Tags = new[] { category };
		}
	}
}