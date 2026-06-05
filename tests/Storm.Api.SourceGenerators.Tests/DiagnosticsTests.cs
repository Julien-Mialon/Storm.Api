using Microsoft.CodeAnalysis;
using Storm.Api.SourceGenerators.Tests.Infrastructure;
using Xunit;

namespace Storm.Api.SourceGenerators.Tests;

/// <summary>
/// Covers the diagnostic / error-handling paths of the generator (the <c>SG0001</c> catch-all in
/// <c>BaseContextTransformer.Transform</c> and graceful no-ops).
/// </summary>
public class DiagnosticsTests
{
	[Fact]
	public void Action_type_that_does_not_implement_IAction_is_skipped_without_a_diagnostic()
	{
		// NotAnAction does not implement IAction<,> -> the transform returns null, no output, no error.
		const string SOURCE = """

			public class NotAnAction { }
			public partial class C : BaseController
			{
				public C(IServiceProvider services) : base(services) { }
				[WithAction<NotAnAction>] public partial Task<ActionResult<Response<string>>> M();
			}

			""";
		GeneratorOutput output = GeneratorTestHelper.Run(Sources.InNamespace(SOURCE));

		Assert.False(output.HasAnyControllerOutput);
		Assert.Empty(output.GeneratorDiagnostics);
	}

	[Fact]
	public void Missing_framework_type_surfaces_an_SG0001_diagnostic()
	{
		// Remove Storm.Api.CQRS.Domains.Results.ApiFileResult from the framework so the eager type
		// resolution in ContextTransformer throws, which is reported as SG0001.
		string brokenFramework = FrameworkStubs.SOURCE
			.Replace("class ApiFileResult", "class RenamedAway");

		const string USER = """

			using System;
			using System.Threading.Tasks;
			using Microsoft.AspNetCore.Mvc;
			using Storm.Api;
			using Storm.Api.CQRS;
			using Storm.Api.Controllers;
			using Storm.Api.Dtos;
			using Storm.Api.SourceGenerators.ActionMethods;

			namespace Sample;

			public class Q : BaseAction<Unit, string>
			{
				public Q(IServiceProvider services) : base(services) { }
				protected override Task<string> Action(Unit parameter) => Task.FromResult("");
			}

			public partial class C : BaseController
			{
				public C(IServiceProvider services) : base(services) { }
				[WithAction<Q>] public partial Task<ActionResult<Response<string>>> M();
			}

			""";
		GeneratorOutput output = GeneratorTestHelper.RunWithFramework(brokenFramework, USER);

		Assert.Contains(output.GeneratorDiagnostics, diagnostic => diagnostic.Id == "SG0001");
		Assert.Contains(output.GeneratorDiagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
	}
}
