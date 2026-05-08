namespace Storm.Api.SourceGenerators.ActionMethods;

internal struct StatusCodeContext : IEquatable<StatusCodeContext>
{
	public int StatusCode;
	public string? Description;


	public bool Equals(StatusCodeContext other)
	{
		return StatusCode == other.StatusCode && Description == other.Description;
	}

	public override bool Equals(object? obj)
	{
		return obj is StatusCodeContext other && Equals(other);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			return (StatusCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
		}
	}
}