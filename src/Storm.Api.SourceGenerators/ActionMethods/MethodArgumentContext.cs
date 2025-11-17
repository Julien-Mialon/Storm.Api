namespace Storm.Api.SourceGenerators.ActionMethods;

internal struct MethodArgumentContext : IEquatable<MethodArgumentContext>
{
	public string Type;
	public string Name;
	public string? MapTo;

	public readonly bool Equals(MethodArgumentContext other)
	{
		return Type == other.Type && Name == other.Name && MapTo == other.MapTo;
	}

	public override readonly bool Equals(object? obj)
	{
		return obj is MethodArgumentContext other && Equals(other);
	}

	public override readonly int GetHashCode()
	{
		unchecked
		{
			var hashCode = Type.GetHashCode();
			hashCode = (hashCode * 397) ^ Name.GetHashCode();
			hashCode = (hashCode * 397) ^ (MapTo != null ? MapTo.GetHashCode() : 0);
			return hashCode;
		}
	}
}