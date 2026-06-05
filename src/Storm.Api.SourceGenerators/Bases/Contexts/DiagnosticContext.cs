using System.Collections.Immutable;

namespace Storm.Api.SourceGenerators.Bases.Contexts;

public struct DiagnosticContext : IEquatable<DiagnosticContext>
{
	public ImmutableArray<DiagnosticItemContext> Items;

	public readonly bool Equals(DiagnosticContext other)
	{
		if (Items.IsDefaultOrEmpty)
		{
			return other.Items.IsDefaultOrEmpty;
		}

		return Items.SequenceEqual(other.Items);
	}

	public override readonly bool Equals(object? obj)
	{
		return obj is DiagnosticContext other && Equals(other);
	}

	public override readonly int GetHashCode()
	{
		if (Items.IsDefaultOrEmpty)
		{
			return 0;
		}

		int hashCode = Items[0].GetHashCode();
		for (int i = 1; i < Items.Length; i++)
		{
			hashCode = (hashCode * 397) ^ Items[i].GetHashCode();
		}

		return hashCode;
	}
}