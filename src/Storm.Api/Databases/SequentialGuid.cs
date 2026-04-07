namespace Storm.Api.Databases;

/// <summary>
/// Generates sequential GUIDs optimized for database index performance.
/// Uses Guid v7 (timestamp-ordered) for databases that sort GUIDs lexicographically (PostgreSQL, MySQL, SQLite),
/// and a COMB GUID variant for SQL Server which sorts uniqueidentifier by bytes 10-15 first.
/// </summary>
public static class SequentialGuid
{
	private static bool _useSqlServerOrdering;
	private static TimeProvider _timeProvider = TimeProvider.System;

	internal static void Initialize(bool useSqlServerOrdering, TimeProvider? timeProvider = null)
	{
		_useSqlServerOrdering = useSqlServerOrdering;
		_timeProvider = timeProvider ?? TimeProvider.System;
	}

	/// <summary>
	/// Generate a new sequential GUID appropriate for the configured database provider.
	/// For PostgreSQL/MySQL/SQLite: standard Guid v7 (timestamp in bytes 0-5, sorts lexicographically).
	/// For SQL Server: COMB GUID with timestamp in bytes 10-15 (SQL Server's highest sort priority).
	/// </summary>
	public static Guid NewGuid()
	{
		if (!_useSqlServerOrdering)
		{
			return Guid.CreateVersion7(_timeProvider.GetUtcNow());
		}

		return CreateSqlServerSequentialGuid();
	}

	public static Guid CreateSqlServerSequentialGuid()
	{
		Guid guid = Guid.CreateVersion7(_timeProvider.GetUtcNow());
		Span<byte> bytes = stackalloc byte[16];
		guid.TryWriteBytes(bytes);

		// Guid v7 stores the 48-bit timestamp in RFC bytes 0-5. In .NET's byte layout
		// (ToByteArray/TryWriteBytes), the first two groups are little-endian:
		//   .NET bytes 0-3 = RFC bytes 3,2,1,0  (int a, little-endian)
		//   .NET bytes 4-5 = RFC bytes 5,4      (short b, little-endian)
		//
		// SQL Server sorts uniqueidentifier by bytes 10-15 first (byte-by-byte, left to right).
		// Swap the timestamp from bytes 0-5 into bytes 10-15, converting back to big-endian
		// so SQL Server's comparison produces chronological ordering.

		byte r0 = bytes[10], r1 = bytes[11], r2 = bytes[12];
		byte r3 = bytes[13], r4 = bytes[14], r5 = bytes[15];

		// .NET 3,2,1,0 → RFC 0,1,2,3 (most significant first)
		bytes[10] = bytes[3];
		bytes[11] = bytes[2];
		bytes[12] = bytes[1];
		bytes[13] = bytes[0];
		// .NET 5,4 → RFC 4,5
		bytes[14] = bytes[5];
		bytes[15] = bytes[4];

		// Move displaced random bytes into the vacated slots
		bytes[0] = r0;
		bytes[1] = r1;
		bytes[2] = r2;
		bytes[3] = r3;
		bytes[4] = r4;
		bytes[5] = r5;

		// change it to V8
		bytes[7] = (byte)((bytes[6] & 0x0F) | 0x80); // set version to 8 (RFC 9562)

		return new(bytes);
	}
}
