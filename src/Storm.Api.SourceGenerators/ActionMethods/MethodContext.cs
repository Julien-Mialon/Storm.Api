using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Storm.Api.SourceGenerators.ActionMethods;

internal struct MethodContext : IEquatable<MethodContext>
{
	public Accessibility Accessibility;
	public string Name;
	public string ReturnType;

	public ImmutableList<MethodArgumentContext> Arguments;

	public string ActionType;
	public string ActionParameterType;
	public string ActionResultType;

	public ITypeSymbol ActionResultTypeSymbol;
	public ActionType Type;

	public readonly bool Equals(MethodContext other)
	{
		return Accessibility == other.Accessibility && Name == other.Name && ReturnType == other.ReturnType && Arguments.Equals(other.Arguments) && ActionType == other.ActionType && ActionParameterType == other.ActionParameterType && ActionResultType == other.ActionResultType;
	}

	public override readonly bool Equals(object? obj)
	{
		return obj is MethodContext other && Equals(other);
	}

	public override readonly int GetHashCode()
	{
		unchecked
		{
			var hashCode = (int)Accessibility;
			hashCode = (hashCode * 397) ^ Name.GetHashCode();
			hashCode = (hashCode * 397) ^ ReturnType.GetHashCode();
			hashCode = (hashCode * 397) ^ Arguments.GetHashCode();
			hashCode = (hashCode * 397) ^ ActionType.GetHashCode();
			hashCode = (hashCode * 397) ^ ActionParameterType.GetHashCode();
			hashCode = (hashCode * 397) ^ ActionResultType.GetHashCode();
			return hashCode;
		}
	}
}