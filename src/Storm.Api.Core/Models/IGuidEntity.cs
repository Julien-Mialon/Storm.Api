using System;

namespace Storm.Api.Core.Models
{
	public interface IGuidEntity : ICommonEntity
	{
		Guid Id { get; set; }
	}
}