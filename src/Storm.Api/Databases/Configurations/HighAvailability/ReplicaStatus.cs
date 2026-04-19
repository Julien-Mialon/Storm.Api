namespace Storm.Api.Databases.Configurations.HighAvailability;

/// <summary>
/// Immutable snapshot of a replica's health and role, as observed by the latest probe.
/// </summary>
/// <param name="Host">Hostname or IP of the replica.</param>
/// <param name="Port">TCP port of the replica.</param>
/// <param name="State">Current reachability state.</param>
/// <param name="Role">Current role of the replica (Primary or Secondary), or Unknown when offline.</param>
/// <param name="LastKnownRole">The role the replica held the last time it was Online.</param>
/// <param name="LastCheckedUtc">Timestamp of the latest probe attempt.</param>
/// <param name="LastError">Error message captured on the latest failed probe, if any.</param>
/// <param name="IsReadEligible">
/// Whether the replica is currently eligible to serve reads.
/// Always true for the Primary; for Secondary replicas this depends on availability mode.
/// </param>
public sealed record ReplicaStatus(
	string Host,
	int Port,
	ReplicaState State,
	ReplicaRole Role,
	ReplicaRole LastKnownRole,
	DateTimeOffset LastCheckedUtc,
	string? LastError,
	bool IsReadEligible);
