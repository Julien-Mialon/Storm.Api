using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Storm.Api.SourceGenerators.ActionMethods;

internal struct GeneratorContext : IEquatable<GeneratorContext>
{
	public string? Namespace;
	public string ClassName;
	public Accessibility ClassAccessibility;
	public ImmutableList<MethodContext> Methods;
	public Types Types;

	public readonly bool Equals(GeneratorContext other)
	{
		return Namespace == other.Namespace && ClassName == other.ClassName && ClassAccessibility == other.ClassAccessibility && Methods.Equals(other.Methods);
	}

	public override readonly bool Equals(object? obj)
	{
		return obj is GeneratorContext other && Equals(other);
	}

	public override readonly int GetHashCode()
	{
		unchecked
		{
			var hashCode = (Namespace != null ? Namespace.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ ClassName.GetHashCode();
			hashCode = (hashCode * 397) ^ (int)ClassAccessibility;
			hashCode = (hashCode * 397) ^ Methods.GetHashCode();
			return hashCode;
		}
	}
}