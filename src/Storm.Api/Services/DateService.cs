namespace Storm.Api.Services;

[Obsolete("This service is deprecated. Please use TimeProvider instead.")]
public class DateService(TimeProvider timeProvider) : IDateService
{
	public DateTime Now => timeProvider.GetUtcNow().UtcDateTime;
}