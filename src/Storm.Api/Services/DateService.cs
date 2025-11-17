namespace Storm.Api.Services;

public class DateService : IDateService
{
	public DateTime Now => DateTime.UtcNow;
}