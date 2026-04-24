using Storm.Api.Extensions;

namespace Storm.Api.Tests.Extensions;

public class DateTimeExtensionsTests
{
	private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
	{
		public override DateTimeOffset GetUtcNow() => now;
	}

	[Fact]
	public void ToTimestamp_NonUtcDateTime_ThrowsInvalidOperation()
	{
		DateTime local = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Local);
		Action act = () => local.ToTimestamp();
		act.Should().Throw<InvalidOperationException>();
	}

	[Fact]
	public void ToTimestamp_Epoch_ReturnsZero()
	{
		DateTime.UnixEpoch.ToTimestamp().Should().Be(0);
	}

	[Fact]
	public void ToTimestamp_KnownDate_ReturnsExpectedUnixSeconds()
	{
		DateTime date = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		date.ToTimestamp().Should().Be(1577836800);
	}

	[Fact]
	public void FromTimestamp_Zero_ReturnsEpoch()
	{
		0L.FromTimestamp().Should().Be(DateTime.UnixEpoch);
	}

	[Fact]
	public void FromTimestamp_Negative_ReturnsEpoch()
	{
		(-5L).FromTimestamp().Should().Be(DateTime.UnixEpoch);
	}

	[Fact]
	public void FromTimestamp_KnownValue_ReturnsExpectedDate()
	{
		long ts = 1577836800;
		ts.FromTimestamp().Should().Be(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));
	}

	[Fact]
	public void IsPast_PastDate_ReturnsTrue()
	{
		FixedTimeProvider tp = new(new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero));
		new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc).IsPast(tp).Should().BeTrue();
	}

	[Fact]
	public void IsPast_FutureDate_ReturnsFalse()
	{
		FixedTimeProvider tp = new(new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero));
		new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc).IsPast(tp).Should().BeFalse();
	}

	[Fact]
	public void IsFuture_FutureDate_ReturnsTrue()
	{
		FixedTimeProvider tp = new(new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero));
		new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc).IsFuture(tp).Should().BeTrue();
	}

	[Fact]
	public void IsToday_SameCalendarDay_ReturnsTrue()
	{
		FixedTimeProvider tp = new(new DateTimeOffset(2024, 6, 1, 15, 0, 0, TimeSpan.Zero));
		new DateTime(2024, 6, 1, 9, 0, 0, DateTimeKind.Utc).IsToday(tp).Should().BeTrue();
	}

	[Fact]
	public void IsToday_DifferentDay_ReturnsFalse()
	{
		FixedTimeProvider tp = new(new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero));
		new DateTime(2024, 6, 2, 0, 0, 0, DateTimeKind.Utc).IsToday(tp).Should().BeFalse();
	}

	[Fact]
	public void IsThisWeek_SameIsoWeek_ReturnsTrue()
	{
		FixedTimeProvider tp = new(new DateTimeOffset(2024, 6, 5, 0, 0, 0, TimeSpan.Zero));
		new DateTime(2024, 6, 3, 0, 0, 0, DateTimeKind.Utc).IsThisWeek(tp).Should().BeTrue();
	}

	[Fact]
	public void IsThisWeek_DifferentWeek_ReturnsFalse()
	{
		FixedTimeProvider tp = new(new DateTimeOffset(2024, 6, 5, 0, 0, 0, TimeSpan.Zero));
		new DateTime(2024, 5, 27, 0, 0, 0, DateTimeKind.Utc).IsThisWeek(tp).Should().BeFalse();
	}

	[Theory]
	[InlineData(2024, 6, 9, 2024, 6, 3)]
	[InlineData(2024, 6, 3, 2024, 6, 3)]
	[InlineData(2024, 6, 4, 2024, 6, 3)]
	[InlineData(2024, 6, 5, 2024, 6, 3)]
	[InlineData(2024, 6, 6, 2024, 6, 3)]
	[InlineData(2024, 6, 7, 2024, 6, 3)]
	[InlineData(2024, 6, 8, 2024, 6, 3)]
	public void AsMonday_KnownDay_ShiftsToMonday(int y, int m, int d, int expY, int expM, int expD)
	{
		new DateTime(y, m, d).AsMonday().Should().Be(new DateTime(expY, expM, expD));
	}

	private sealed class TestRange(DateTime start, DateTime end) : IDateRange
	{
		public DateTime StartTime { get; } = start;
		public DateTime EndTime { get; } = end;
	}

	[Fact]
	public void TodayIsInRange_NullRange_ReturnsFalse()
	{
		IDateRange? range = null;
		range.TodayIsInRange(DateTime.UtcNow).Should().BeFalse();
	}

	[Fact]
	public void TodayIsInRange_WithinRange_ReturnsTrue()
	{
		TestRange range = new(new DateTime(2024, 1, 1), new DateTime(2024, 12, 31));
		range.TodayIsInRange(new DateTime(2024, 6, 1)).Should().BeTrue();
	}

	[Fact]
	public void TodayIsInRange_BoundaryInclusive_BehavesAsImplemented()
	{
		TestRange range = new(new DateTime(2024, 1, 1), new DateTime(2024, 12, 31));
		range.TodayIsInRange(new DateTime(2024, 1, 1)).Should().BeTrue();
		range.TodayIsInRange(new DateTime(2024, 12, 31)).Should().BeTrue();
	}
}
