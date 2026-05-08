namespace Storm.Api.SourceGenerators.ActionMethods;

internal struct ErrorCodeContext : IEquatable<ErrorCodeContext>
{
	public string ErrorCode;
	public string? Description;


	public bool Equals(ErrorCodeContext other)
	{
		return ErrorCode == other.ErrorCode && Description == other.Description;
	}

	public override bool Equals(object? obj)
	{
		return obj is ErrorCodeContext other && Equals(other);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			return (ErrorCode.GetHashCode() * 397) ^ (Description != null ? Description.GetHashCode() : 0);
		}
	}
}