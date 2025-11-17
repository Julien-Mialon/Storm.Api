using Storm.Api.Databases.Models;

namespace Storm.Api.Sample;

public class SampleEntity : BaseEntityWithAutoIncrement
{
	public string? Name { get; set; }
}