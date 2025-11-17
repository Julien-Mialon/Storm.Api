using Microsoft.CodeAnalysis;

namespace Storm.Api.SourceGenerators.Bases;

public struct DiagnosticItemContext : IEquatable<DiagnosticItemContext>
{
	public string Id = "SG0001";
	public string Title = string.Empty;
	public string MessageFormat = string.Empty;
	public string Category = "SourceGenerator";
	public DiagnosticSeverity Severity = DiagnosticSeverity.Error;
	public Location Location = Location.None;

	public DiagnosticItemContext()
	{
	}

	public readonly bool Equals(DiagnosticItemContext other)
	{
		return Id == other.Id && Title == other.Title && MessageFormat == other.MessageFormat && Category == other.Category && Severity == other.Severity && Location.Equals(other.Location);
	}

	public override readonly bool Equals(object? obj)
	{
		return obj is DiagnosticItemContext other && Equals(other);
	}

	public override readonly int GetHashCode()
	{
		unchecked
		{
			var hashCode = Id.GetHashCode();
			hashCode = (hashCode * 397) ^ Title.GetHashCode();
			hashCode = (hashCode * 397) ^ MessageFormat.GetHashCode();
			hashCode = (hashCode * 397) ^ Category.GetHashCode();
			hashCode = (hashCode * 397) ^ (int)Severity;
			hashCode = (hashCode * 397) ^ Location.GetHashCode();
			return hashCode;
		}
	}
}