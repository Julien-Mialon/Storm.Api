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
	public INamedTypeSymbol SuccessCodeAttribute;
	public INamedTypeSymbol ErrorCodeAttribute;
	public INamedTypeSymbol HttpErrorAttribute;
	public INamedTypeSymbol DescriptionAttribute;
	public INamedTypeSymbol SummaryAttribute;
	public INamedTypeSymbol MediaTypeAttribute;
	public INamedTypeSymbol InternalActionCallAttribute;

	public INamedTypeSymbol OpenApiErrorCodesAttribute;
	public INamedTypeSymbol IAction;

	public INamedTypeSymbol TaskT;
	public INamedTypeSymbol AspNetIActionResult;
	public INamedTypeSymbol AspNetActionResultT;
	public INamedTypeSymbol AspNetFileResult;

	public INamedTypeSymbol ProducesResponseTypeAttribute;
	public INamedTypeSymbol EndpointSummaryAttribute;
	public INamedTypeSymbol EndpointDescriptionAttribute;

}