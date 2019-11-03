using System;

namespace Storm.Api.Core.Services
{
	public interface IDateService
	{
		DateTime Now { get; }
	}

	public class DateService : IDateService
	{
		public DateTime Now => DateTime.UtcNow;
	}
}