namespace Storm.Api.Databases.Configurations.HighAvailability;

/// <summary>
/// User-tunable options for the SQL Server HA layer. Only consumed when at least one
/// read replica has been registered on <see cref="DatabaseConfigurationBuilder"/>.
/// </summary>
public sealed class HighAvailabilityOptions
{
	/// <summary>Interval between replica health probes. Default: 5 seconds.</summary>
	public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromSeconds(5);

	/// <summary>Connect timeout used for each probe attempt. Default: 2 seconds.</summary>
	public TimeSpan ProbeConnectTimeout { get; set; } = TimeSpan.FromSeconds(2);

	/// <summary>
	/// When false (default), read routing only considers secondary replicas verified to be in
	/// SYNCHRONOUS_COMMIT + SYNCHRONIZED mode. Set to true if your secondaries use asynchronous
	/// commit and you're willing to accept potentially stale reads.
	/// </summary>
	public bool AllowReadsOnAsyncReplicas { get; set; }

	/// <summary>
	/// When true (default), <c>UseReadConnection</c> falls back to the primary when no eligible
	/// secondary is available. Set to false to make reads fail fast with <c>DomainDatabaseException</c>.
	/// </summary>
	public bool AllowReadFallbackToPrimary { get; set; } = true;

	public OnPrimarySwitched? PrimarySwitched { get; set; }
	public OnPrimaryUnavailable? PrimaryUnavailable { get; set; }
	public OnPrimaryRestored? PrimaryRestored { get; set; }
	public OnReplicaStateChanged? ReplicaStateChanged { get; set; }
}
