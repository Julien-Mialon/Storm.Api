using Microsoft.CodeAnalysis;

namespace Storm.Api.SourceGenerators.ActionMethods;

internal struct Types
{
	public INamedTypeSymbol Unit;
	public INamedTypeSymbol Response;
	public INamedTypeSymbol ResponseT;
	public INamedTypeSymbol ApiFileResult;
	public INamedTypeSymbol WithActionAttribute;
	public INamedTypeSymbol MapToAttribute;
	public INamedTypeSymbol IAction;

	public INamedTypeSymbol TaskT;
	public INamedTypeSymbol AspNetIActionResult;
	public INamedTypeSymbol AspNetActionResultT;
}