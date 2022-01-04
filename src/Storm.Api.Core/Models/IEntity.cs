namespace Storm.Api.Core.Models;

public interface IEntity : ICommonEntity
{
	long Id { get; set; }
	Guid CollationId { get; set; }
}