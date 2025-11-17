using System.Collections.Immutable;

namespace Storm.Api.SourceGenerators.Bases;

public struct DiagnosticContext : IEquatable<DiagnosticContext>
{
	public ImmutableList<DiagnosticItemContext> Items;

	public readonly bool Equals(DiagnosticContext other)
	{
		return Items.Equals(other.Items);
	}

	public override readonly bool Equals(object? obj)
	{
		return obj is DiagnosticContext other && Equals(other);
	}

	public override readonly int GetHashCode()
	{
		return Items.GetHashCode();
	}
}