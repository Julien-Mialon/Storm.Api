namespace Storm.Api.Core.Models;

public interface IGuidEntity : IDeletableEntity
{
	Guid Id { get; set; }
}