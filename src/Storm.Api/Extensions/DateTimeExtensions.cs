namespace Storm.Api.Extensions;

public interface IDateRange
{
	DateTime StartTime { get; }

	DateTime EndTime { get; }
}

public static class DateTimeExtensions
{
	public static readonly DateTime DEFAULT = new(2000, 1, 1);

	public static long ToTimestamp(this DateTime dateTime)
	{
		if (dateTime.Kind != DateTimeKind.Utc)
		{
			throw new InvalidOperationException("Only compute Timestamp on UTC date!");
		}

		return (long)dateTime.Subtract(DateTime.UnixEpoch).TotalSeconds;
	}

	public static DateTime FromTimestamp(this long timestamp)
	{
		if (timestamp <= 0)
		{
			return DateTime.UnixEpoch;
		}

		return DateTime.UnixEpoch.AddSeconds(timestamp);
	}

	extension(DateTime date)
	{
		public bool IsPast()
			=> date.IsPast(TimeProvider.System);

		public bool IsPast(TimeProvider timeProvider)
		{
			return date < timeProvider.GetUtcNow().UtcDateTime;
		}

		public bool IsFuture()
			=> date.IsFuture(TimeProvider.System);

		public bool IsFuture(TimeProvider timeProvider)
		{
			return date > timeProvider.GetUtcNow().UtcDateTime;
		}

		public bool IsToday()
			=> date.IsToday(TimeProvider.System);

		public bool IsToday(TimeProvider timeProvider)
		{
			return date.Date == timeProvider.GetUtcNow().UtcDateTime.Date;
		}

		public bool IsNotToday()
			=> date.IsNotToday(TimeProvider.System);

		public bool IsNotToday(TimeProvider timeProvider)
		{
			return date.Date != timeProvider.GetUtcNow().UtcDateTime.Date;
		}

		public bool IsThisWeek()
			=> date.IsThisWeek(TimeProvider.System);

		public bool IsThisWeek(TimeProvider timeProvider)
		{
			return date.AsMonday() == timeProvider.GetUtcNow().UtcDateTime.AsMonday();
		}

		public bool IsNotThisWeek()
			=> date.IsNotThisWeek(TimeProvider.System);

		public bool IsNotThisWeek(TimeProvider timeProvider)
		{
			return date.AsMonday() != timeProvider.GetUtcNow().UtcDateTime.AsMonday();
		}

		public DateTime AsMonday()
		{
			int daysToRemove = date.DayOfWeek switch
			{
				DayOfWeek.Sunday => -6,
				DayOfWeek.Monday => 0,
				DayOfWeek.Tuesday => -1,
				DayOfWeek.Wednesday => -2,
				DayOfWeek.Thursday => -3,
				DayOfWeek.Friday => -4,
				DayOfWeek.Saturday => -5,
				_ => 0,
			};

			return date.Date.AddDays(daysToRemove);
		}
	}

	extension(IDateRange? date)
	{
		public bool TodayIsInRange(DateTime now)
		{
			if (date is null)
			{
				return false;
			}

			return date.StartTime <= now && now <= date.EndTime;
		}

		public bool TodayIsNotInRange(DateTime now)
		{
			if (date is null)
			{
				return false;
			}

			return date.StartTime > now || now > date.EndTime;
		}
	}
}