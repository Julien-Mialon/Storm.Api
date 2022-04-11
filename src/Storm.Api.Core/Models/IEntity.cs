namespace Storm.Api.Core.Models;

public interface IEntity : IDeletableEntity
{
	long Id { get; set; }
	Guid CollationId { get; set; }
}