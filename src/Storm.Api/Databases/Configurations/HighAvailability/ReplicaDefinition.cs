namespace Storm.Api.Databases.Configurations.HighAvailability;

/// <summary>
/// Static definition of a replica node as declared at startup.
/// The manager pairs this with a mutable <see cref="ReplicaStatus"/> snapshot.
/// </summary>
internal sealed record ReplicaDefinition(string Host, int Port, string ConnectionString);
