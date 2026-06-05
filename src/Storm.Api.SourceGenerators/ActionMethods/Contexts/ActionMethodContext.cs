using Microsoft.CodeAnalysis;

namespace Storm.Api.SourceGenerators.ActionMethods.Contexts;

/// <summary>
/// Per-method result produced by the <see cref="ContextTransformer"/> when a method is decorated with
/// <c>[WithAction&lt;T&gt;]</c>. These are grouped back by their containing class into a
/// <see cref="GeneratorContext"/> before code generation.
/// </summary>
internal struct ActionMethodContext : IEquatable<ActionMethodContext>
{
	public string? Namespace;
	public string ClassName;
	public Accessibility ClassAccessibility;
	public MethodContext Method;

	public readonly bool Equals(ActionMethodContext other)
	{
		return Namespace == other.Namespace
			&& ClassName == other.ClassName
			&& ClassAccessibility == other.ClassAccessibility
			&& Method.Equals(other.Method);
	}

	public override readonly bool Equals(object? obj)
	{
		return obj is ActionMethodContext other && Equals(other);
	}

	public override readonly int GetHashCode()
	{
		unchecked
		{
			int hashCode = Namespace != null ? Namespace.GetHashCode() : 0;
			hashCode = (hashCode * 397) ^ ClassName.GetHashCode();
			hashCode = (hashCode * 397) ^ (int)ClassAccessibility;
			hashCode = (hashCode * 397) ^ Method.GetHashCode();
			return hashCode;
		}
	}
}
