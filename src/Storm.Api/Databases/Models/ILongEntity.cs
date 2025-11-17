namespace Storm.Api.Databases.Models;

public interface ILongEntity
{
	long Id { get; set; }
	Guid CollationId { get; set; }
}