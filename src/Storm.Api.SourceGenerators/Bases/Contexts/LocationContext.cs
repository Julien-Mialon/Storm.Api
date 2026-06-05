using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Storm.Api.SourceGenerators.Bases.Contexts;

public struct LocationContext : IEquatable<LocationContext>
{
	public string FilePath = string.Empty;
	public LinePositionSpan LinePositionSpan;
	public TextSpan TextSpan;

	public LocationContext(Location source)
	{
		FilePath = source.SourceTree?.FilePath ?? string.Empty;
		TextSpan = source.SourceSpan;
		LinePositionSpan = source.GetLineSpan().Span;
	}

	public readonly Location ToLocation()
	{
		return Location.Create(FilePath, TextSpan, LinePositionSpan);
	}

	public bool Equals(LocationContext other)
	{
		return FilePath == other.FilePath && LinePositionSpan.Equals(other.LinePositionSpan) && TextSpan.Equals(other.TextSpan);
	}

	public override bool Equals(object? obj)
	{
		return obj is LocationContext other && Equals(other);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = FilePath.GetHashCode();
			hashCode = (hashCode * 397) ^ LinePositionSpan.GetHashCode();
			hashCode = (hashCode * 397) ^ TextSpan.GetHashCode();
			return hashCode;
		}
	}
}