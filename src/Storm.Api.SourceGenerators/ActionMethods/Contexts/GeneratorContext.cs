using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Storm.Api.SourceGenerators.ActionMethods.Contexts;

internal struct GeneratorContext : IEquatable<GeneratorContext>
{
	public string? Namespace;
	public string ClassName;
	public Accessibility ClassAccessibility;
	public ImmutableArray<MethodContext> Methods;
	public Types Types; //TODO: review this one, it shouldn't be here

	public readonly bool Equals(GeneratorContext other)
	{
		return Namespace == other.Namespace
			&& ClassName == other.ClassName
			&& ClassAccessibility == other.ClassAccessibility
			&& Methods.SequenceEqual(other.Methods);
	}

	public override readonly bool Equals(object? obj)
	{
		return obj is GeneratorContext other && Equals(other);
	}

	public override readonly int GetHashCode()
	{
		unchecked
		{
			int hashCode = Namespace != null ? Namespace.GetHashCode() : 0;
			hashCode = (hashCode * 397) ^ ClassName.GetHashCode();
			hashCode = (hashCode * 397) ^ (int)ClassAccessibility;
			foreach (MethodContext methodContext in Methods)
			{
				hashCode = (hashCode * 397) ^ methodContext.GetHashCode();
			}
			return hashCode;
		}
	}
}