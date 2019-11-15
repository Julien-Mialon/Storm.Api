using ServiceStack.DataAnnotations;
using Storm.Api.Core.Models;

namespace Storm.SqlMigrations
{
	[Alias("Migrations")]
	public class Migration : BaseEntityWithAutoIncrement
	{
		[Index]
		public int Number { get; set; }

		[Index]
		[StringLength(200)]
		public string Module { get; set; }
	}
}