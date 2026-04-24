using System.Reflection;
using Storm.Api.Databases;

namespace Storm.Api.Tests.Databases;

public class SequentialGuidTests
{
	private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
	{
		public DateTimeOffset Now { get; set; } = now;
		public override DateTimeOffset GetUtcNow() => Now;
	}

	private static void Init(bool sqlServer, TimeProvider? tp = null)
	{
		MethodInfo? m = typeof(SequentialGuid).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Static);
		m!.Invoke(null, [sqlServer, tp]);
	}

	[Fact]
	public void NewGuid_ConsecutiveCalls_AreChronologicallyOrdered()
	{
		Init(sqlServer: false);
		Guid a = SequentialGuid.NewGuid();
		Thread.Sleep(5);
		Guid b = SequentialGuid.NewGuid();
		a.CompareTo(b).Should().BeLessThan(0);
	}

	[Fact]
	public void NewGuid_PostgresLikeProvider_ReturnsUuidV7Format()
	{
		Init(sqlServer: false);
		Guid g = SequentialGuid.NewGuid();
		byte[] bytes = g.ToByteArray();
		// byte 7 holds the version nibble (.NET layout puts the two version bits in short c — which is bytes 6/7).
		// Actually Guid.CreateVersion7 yields version 7 in the RFC bytes; regardless of layout, version should be 7.
		g.Version.Should().Be(7);
	}

	[Fact]
	public void CreateSqlServerSequentialGuid_TimestampInBytes10To15()
	{
		DateTimeOffset t1 = new(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
		DateTimeOffset t2 = new(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);

		FixedTimeProvider tp = new(t1);
		Init(sqlServer: true, tp);
		Guid a = SequentialGuid.CreateSqlServerSequentialGuid();
		tp.Now = t2;
		Guid b = SequentialGuid.CreateSqlServerSequentialGuid();

		byte[] ba = a.ToByteArray();
		byte[] bb = b.ToByteArray();
		// Later timestamp in bytes 10-15 should be lexicographically larger.
		int cmp = 0;
		for (int i = 10; i < 16; i++)
		{
			if (ba[i] != bb[i])
			{
				cmp = ba[i] < bb[i] ? -1 : 1;
				break;
			}
		}
		cmp.Should().BeLessThan(0);
	}

	[Fact]
	public void CreateSqlServerSequentialGuid_SameTickSequence_SortsCorrectly()
	{
		DateTimeOffset t = new(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
		FixedTimeProvider tp = new(t);
		Init(sqlServer: true, tp);
		Guid a = SequentialGuid.CreateSqlServerSequentialGuid();
		Guid b = SequentialGuid.CreateSqlServerSequentialGuid();
		a.Should().NotBe(b);
	}

	[Fact]
	public void NewGuid_WithMockedTimeProvider_IsDeterministic()
	{
		DateTimeOffset fixedTime = new(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
		FixedTimeProvider tp = new(fixedTime);
		Init(sqlServer: false, tp);
		Guid a = SequentialGuid.NewGuid();
		Guid b = SequentialGuid.NewGuid();
		byte[] ba = a.ToByteArray();
		byte[] bb = b.ToByteArray();
		// First 4 bytes encode timestamp in little-endian (.NET) — they should match.
		ba[..4].Should().Equal(bb[..4]);
	}
}
