namespace Storm.Api.Databases.Configurations.HighAvailability;

/// <summary>Fires once when the manager observes a different primary than before.</summary>
public delegate void OnPrimarySwitched(ReplicaStatus previousPrimary, ReplicaStatus newPrimary);

/// <summary>Fires the first probe after the primary becomes unreachable.</summary>
public delegate void OnPrimaryUnavailable(ReplicaStatus? lastKnownPrimary);

/// <summary>Fires the first probe after a primary becomes reachable again.</summary>
public delegate void OnPrimaryRestored(ReplicaStatus primary);

/// <summary>Fires on any per-replica state or role transition.</summary>
public delegate void OnReplicaStateChanged(ReplicaStatus previous, ReplicaStatus current);
