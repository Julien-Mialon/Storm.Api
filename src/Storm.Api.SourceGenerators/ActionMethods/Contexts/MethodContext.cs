using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Storm.Api.SourceGenerators.ActionMethods.Contexts;

internal struct MethodContext : IEquatable<MethodContext>
{
	public Accessibility Accessibility;
	public string Name;
	public string ReturnType;
	public string OpenApiReturnType;

	public ImmutableArray<MethodArgumentContext> Arguments;

	public string ActionType;
	public string ActionParameterType;
	public string ActionResultType;

	public ITypeSymbol ActionResultTypeSymbol; //TODO: should be removed
	public ActionType Type;

	public string? Summary;
	public string? Description;
	public ImmutableArray<ErrorCodeContext> ErrorCodes;
	public ImmutableArray<StatusCodeContext> HttpErrorStatusCodes;
	public ImmutableArray<StatusCodeContext> SuccessStatusCodes;
	public string? MediaType;


	public readonly bool Equals(MethodContext other)
	{
		return Accessibility == other.Accessibility
			&& Name == other.Name
			&& ReturnType == other.ReturnType
			&& OpenApiReturnType == other.OpenApiReturnType
			&& Arguments.SequenceEqual(other.Arguments)
			&& ActionType == other.ActionType
			&& ActionParameterType == other.ActionParameterType
			&& ActionResultType == other.ActionResultType
			&& Summary == other.Summary
			&& Description == other.Description
			&& ErrorCodes.SequenceEqual(other.ErrorCodes)
			&& HttpErrorStatusCodes.SequenceEqual(other.HttpErrorStatusCodes)
			&& SuccessStatusCodes.SequenceEqual(other.SuccessStatusCodes)
			&& MediaType == other.MediaType;
	}

	public override readonly bool Equals(object? obj)
	{
		return obj is MethodContext other && Equals(other);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = (int)Accessibility;
			hashCode = (hashCode * 397) ^ Name.GetHashCode();
			hashCode = (hashCode * 397) ^ ReturnType.GetHashCode();
			hashCode = (hashCode * 397) ^ OpenApiReturnType.GetHashCode();
			foreach (MethodArgumentContext argument in Arguments)
			{
				hashCode = (hashCode * 397) ^ argument.GetHashCode();
			}
			hashCode = (hashCode * 397) ^ ActionType.GetHashCode();
			hashCode = (hashCode * 397) ^ ActionParameterType.GetHashCode();
			hashCode = (hashCode * 397) ^ ActionResultType.GetHashCode();
			hashCode = (hashCode * 397) ^ (int)Type;
			hashCode = (hashCode * 397) ^ (Summary?.GetHashCode() ?? 0);
			hashCode = (hashCode * 397) ^ (Description?.GetHashCode() ?? 0);
			foreach (ErrorCodeContext errorCode in ErrorCodes)
			{
				hashCode = (hashCode * 397) ^ errorCode.GetHashCode();
			}
			foreach (StatusCodeContext statusCode in HttpErrorStatusCodes)
			{
				hashCode = (hashCode * 397) ^ statusCode.GetHashCode();
			}
			foreach (StatusCodeContext statusCode in SuccessStatusCodes)
			{
				hashCode = (hashCode * 397) ^ statusCode.GetHashCode();
			}
			hashCode = (hashCode * 397) ^ (MediaType?.GetHashCode() ?? 0);
			return hashCode;
		}
	}
}