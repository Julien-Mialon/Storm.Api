using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace Storm.Api.SourceGenerators.ActionMethods;

/// <summary>
/// Resolves the framework symbols the transform needs from a <see cref="Compilation"/>, lazily and
/// at most once per compilation.
/// <para>
/// Each type is looked up on first access and memoized (#8): a method whose code path never touches
/// a given type never resolves it — so e.g. a <c>Unit</c> or file endpoint does not require
/// <c>Response&lt;T&gt;</c> to be present, and a missing-but-unused framework type no longer fails
/// every decorated method with an <c>SG0001</c>.
/// </para>
/// <para>
/// Instances are cached per compilation via a <see cref="ConditionalWeakTable{TKey,TValue}"/> (#7),
/// so the ~18 <see cref="Compilation.GetTypeByMetadataName"/> lookups happen once per compilation
/// rather than once per decorated method on every keystroke. The weak key means we do not root the
/// compilation, and these symbols are only ever used transiently inside the transform — they are
/// never stored on the cached model (see source-generator-issue.md #1).
/// </para>
/// </summary>
internal sealed class Types
{
	private static readonly ConditionalWeakTable<Compilation, Types> Cache = new();

	private readonly Compilation _compilation;

	private Types(Compilation compilation)
	{
		_compilation = compilation;
	}

	/// <summary>Returns the cached instance for <paramref name="compilation"/>, creating it on first use.</summary>
	public static Types For(Compilation compilation)
		=> Cache.GetValue(compilation, static c => new Types(c));

	// Idempotent: GetTypeByMetadataName returns the same canonical symbol for a given name, so the
	// lazy `??=` below is safe even if two transforms race on the same instance.
	private INamedTypeSymbol Require(string metadataName)
		=> _compilation.GetTypeByMetadataName(metadataName)
			?? throw new($"Type {metadataName} not found");

	private INamedTypeSymbol? _unit;
	public INamedTypeSymbol Unit => _unit ??= Require("Storm.Api.Unit");

	private INamedTypeSymbol? _response;
	public INamedTypeSymbol Response => _response ??= Require("Storm.Api.Dtos.Response");

	private INamedTypeSymbol? _responseT;
	public INamedTypeSymbol ResponseT => _responseT ??= Require("Storm.Api.Dtos.Response`1");

	private INamedTypeSymbol? _apiFileResult;
	public INamedTypeSymbol ApiFileResult => _apiFileResult ??= Require("Storm.Api.CQRS.Domains.Results.ApiFileResult");

	private INamedTypeSymbol? _iAction;
	public INamedTypeSymbol IAction => _iAction ??= Require("Storm.Api.CQRS.IAction`2");

	private INamedTypeSymbol? _taskT;
	public INamedTypeSymbol TaskT => _taskT ??= Require("System.Threading.Tasks.Task`1");

	private INamedTypeSymbol? _aspNetIActionResult;
	public INamedTypeSymbol AspNetIActionResult => _aspNetIActionResult ??= Require("Microsoft.AspNetCore.Mvc.IActionResult");

	private INamedTypeSymbol? _aspNetActionResultT;
	public INamedTypeSymbol AspNetActionResultT => _aspNetActionResultT ??= Require("Microsoft.AspNetCore.Mvc.ActionResult`1");

	private INamedTypeSymbol? _aspNetFileResult;
	public INamedTypeSymbol AspNetFileResult => _aspNetFileResult ??= Require("System.IO.Stream");

	private INamedTypeSymbol? _withActionAttribute;
	public INamedTypeSymbol WithActionAttribute => _withActionAttribute ??= Require(ActionMethodConstants.WITH_ACTION_ATTRIBUTE.MetadataName);

	private INamedTypeSymbol? _mapToAttribute;
	public INamedTypeSymbol MapToAttribute => _mapToAttribute ??= Require(ActionMethodConstants.MAP_TO_ATTRIBUTE.MetadataName);

	private INamedTypeSymbol? _successCodeAttribute;
	public INamedTypeSymbol SuccessCodeAttribute => _successCodeAttribute ??= Require(ActionMethodConstants.SUCCESS_CODE_ATTRIBUTE.MetadataName);

	private INamedTypeSymbol? _errorCodeAttribute;
	public INamedTypeSymbol ErrorCodeAttribute => _errorCodeAttribute ??= Require(ActionMethodConstants.ERROR_CODE_ATTRIBUTE.MetadataName);

	private INamedTypeSymbol? _httpErrorAttribute;
	public INamedTypeSymbol HttpErrorAttribute => _httpErrorAttribute ??= Require(ActionMethodConstants.HTTP_ERROR_ATTRIBUTE.MetadataName);

	private INamedTypeSymbol? _descriptionAttribute;
	public INamedTypeSymbol DescriptionAttribute => _descriptionAttribute ??= Require(ActionMethodConstants.DESCRIPTION_ATTRIBUTE.MetadataName);

	private INamedTypeSymbol? _summaryAttribute;
	public INamedTypeSymbol SummaryAttribute => _summaryAttribute ??= Require(ActionMethodConstants.SUMMARY_ATTRIBUTE.MetadataName);

	private INamedTypeSymbol? _mediaTypeAttribute;
	public INamedTypeSymbol MediaTypeAttribute => _mediaTypeAttribute ??= Require(ActionMethodConstants.MEDIA_TYPE_ATTRIBUTE.MetadataName);

	private INamedTypeSymbol? _internalActionCallAttribute;
	public INamedTypeSymbol InternalActionCallAttribute => _internalActionCallAttribute ??= Require(ActionMethodConstants.INTERNAL_ACTION_CALL_ATTRIBUTE.MetadataName);
}
