using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Storm.Api.SourceGenerators.ActionMethods;

namespace Storm.Api.Tests.SourceGenerators;

public class ActionMethodCodeGeneratorTests
{
	private static MetadataReference[] DefaultReferences()
	{
		// Ensure types referenced by the generator are loaded into the test AppDomain.
		_ = typeof(Storm.Api.Unit);
		_ = typeof(Storm.Api.Dtos.Response);
		_ = typeof(Storm.Api.Dtos.Response<>);
		_ = typeof(Storm.Api.CQRS.IAction<,>);
		_ = typeof(Storm.Api.CQRS.Domains.Results.ApiFileResult);
		_ = typeof(Microsoft.AspNetCore.Mvc.IActionResult);
		_ = typeof(Microsoft.AspNetCore.Mvc.ActionResult<>);
		_ = typeof(Microsoft.AspNetCore.Mvc.FileResult);
		_ = typeof(Microsoft.AspNetCore.Mvc.ControllerBase);
		_ = typeof(Storm.Api.Controllers.BaseController);

		string trustedPlatformAssemblies = (string)AppDomain.CurrentDomain.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
		HashSet<string> trusted = trustedPlatformAssemblies
			.Split(Path.PathSeparator)
			.Where(p => !string.IsNullOrWhiteSpace(p))
			.ToHashSet();

		IEnumerable<string> loaded = AppDomain.CurrentDomain.GetAssemblies()
			.Where(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
			.Select(a => a.Location);

		HashSet<string> all = [.. trusted, .. loaded];
		return all.Select(p => (MetadataReference)MetadataReference.CreateFromFile(p)).ToArray();
	}

	private static GeneratorDriverRunResult Run(string source)
	{
		CSharpCompilation compilation = CSharpCompilation.Create(
			"TestAssembly",
			[CSharpSyntaxTree.ParseText(source)],
			DefaultReferences(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		ActionMethodCodeGenerator gen = new();
		GeneratorDriver driver = CSharpGeneratorDriver.Create(gen);
		return driver.RunGenerators(compilation).GetRunResult();
	}

	private const string SimpleAction = """
		using Storm.Api.CQRS;
		using Storm.Api.SourceGenerators.ActionMethods;

		namespace TestNs;

		public class MyParam { public string Name { get; set; } = ""; }
		public class MyAction : IAction<MyParam, string>
		{
			public System.Threading.Tasks.Task<string> Execute(MyParam p) => System.Threading.Tasks.Task.FromResult(p.Name);
		}

		public partial class MyController : Storm.Api.Controllers.BaseController
		{
			public MyController(System.IServiceProvider services) : base(services) { }

			[WithAction<MyAction>]
			public partial System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Storm.Api.Dtos.Response<string>>> Do(string name);
		}
		""";

	[Fact]
	public void Generator_PartialClass_WithWithActionAttribute_EmitsSource()
	{
		GeneratorDriverRunResult result = Run(SimpleAction);
		result.GeneratedTrees.Should().Contain(t => t.FilePath.Contains("MyController"));
	}

	[Fact]
	public void Generator_NonPartialClass_DoesNotEmit()
	{
		string src = SimpleAction.Replace("partial class MyController", "class MyController");
		GeneratorDriverRunResult result = Run(src);
		result.GeneratedTrees.Should().NotContain(t => t.FilePath.Contains("MyController.Storm.Api.ActionMethods"));
	}

	[Fact]
	public void Generator_ClassWithoutWithActionAttribute_DoesNotEmit()
	{
		string src = """
			namespace TestNs;
			public partial class Plain { public partial void Noop(); }
			""";
		GeneratorDriverRunResult result = Run(src);
		result.GeneratedTrees.Should().NotContain(t => t.FilePath.Contains("Plain.Storm.Api.ActionMethods"));
	}

	[Fact]
	public void Generator_NonClassNode_Skipped()
	{
		string src = "namespace TestNs; public interface IThing {}";
		GeneratorDriverRunResult result = Run(src);
		result.GeneratedTrees.Should().NotContain(t => t.FilePath.Contains("IThing"));
	}

	[Fact]
	public void Generator_MultipleClassesInFile_EmitsOnePerEligibleClass()
	{
		string src = SimpleAction + """

			public partial class OtherController : Storm.Api.Controllers.BaseController
			{
				public OtherController(System.IServiceProvider services) : base(services) { }
				[WithAction<MyAction>]
				public partial System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Storm.Api.Dtos.Response<string>>> Do(string name);
			}
			""";
		GeneratorDriverRunResult result = Run(src);
		result.GeneratedTrees.Count(t => t.FilePath.Contains("MyController") || t.FilePath.Contains("OtherController")).Should().BeGreaterThan(1);
	}

	private static MetadataReference[] MinimalReferences()
	{
		// Only core framework references — Storm.Api.* excluded so the generator's type lookup fails
		// and BaseContextTransformer converts the thrown exception to an SG0001 diagnostic.
		string trusted = (string)AppDomain.CurrentDomain.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
		return trusted.Split(Path.PathSeparator)
			.Where(p => !string.IsNullOrWhiteSpace(p))
			.Where(p => !Path.GetFileName(p).StartsWith("Storm.Api", StringComparison.OrdinalIgnoreCase))
			.Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
			.ToArray();
	}

	[Fact]
	public void Generator_TransformationThrows_ReportsDiagnostic()
	{
		// Syntax matches the CouldBeAClassToGenerate filter (partial class + method with a name containing
		// "WithAction") but the required Storm.Api types are not in references, so GetRequiredTypeByMetadataName
		// throws and BaseContextTransformer converts it to an SG0001 diagnostic.
		string src = """
			namespace TestNs;
			public partial class C
			{
				[WithAction<object>]
				public partial void Do();
			}
			""";
		CSharpCompilation compilation = CSharpCompilation.Create(
			"TestAssembly",
			[CSharpSyntaxTree.ParseText(src)],
			MinimalReferences(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		GeneratorDriver driver = CSharpGeneratorDriver.Create(new ActionMethodCodeGenerator());
		GeneratorDriverRunResult result = driver.RunGenerators(compilation).GetRunResult();

		result.Diagnostics.Should().Contain(d => d.Id == "SG0001");
		result.GeneratedTrees.Should().NotContain(t => t.FilePath.Contains("C.Storm.Api.ActionMethods"));
	}

	[Fact]
	public void Generator_ActionNotImplementingIAction_MethodSkipped()
	{
		// Action type "NotAnAction" does not implement IAction<,>, so the method is skipped silently.
		// No generated output for the controller, but no diagnostic either.
		string src = """
			using Storm.Api.SourceGenerators.ActionMethods;

			namespace TestNs;

			public class NotAnAction {}

			public partial class PlainController : Storm.Api.Controllers.BaseController
			{
				public PlainController(System.IServiceProvider services) : base(services) { }

				[WithAction<NotAnAction>]
				public partial System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Storm.Api.Dtos.Response<string>>> Do();
			}
			""";
		GeneratorDriverRunResult result = Run(src);

		// The file may still be emitted (class passed the syntax filter) but it must not contain a generated
		// body for the skipped method. The simplest observable signal: no "Services.ExecuteAction" call.
		System.Collections.Generic.IEnumerable<SyntaxTree> trees = result.GeneratedTrees.Where(t => t.FilePath.Contains("PlainController"));
		foreach (SyntaxTree tree in trees)
		{
			tree.ToString().Should().NotContain("Services.ExecuteAction");
		}
	}

	[Fact]
	public void Generator_UnmatchedActionProperty_GeneratesWithoutIt()
	{
		// Action parameter has properties {Name, Ignored}. Method has only a Name arg.
		// Generator should emit an object initializer with Name = name and skip Ignored.
		string src = """
			using Storm.Api.CQRS;
			using Storm.Api.SourceGenerators.ActionMethods;

			namespace TestNs;

			public class ParamWithExtraProp
			{
				public string Name { get; set; } = "";
				public string Ignored { get; set; } = "";
			}

			public class ActionX : IAction<ParamWithExtraProp, string>
			{
				public System.Threading.Tasks.Task<string> Execute(ParamWithExtraProp p) => System.Threading.Tasks.Task.FromResult(p.Name);
			}

			public partial class ControllerX : Storm.Api.Controllers.BaseController
			{
				public ControllerX(System.IServiceProvider services) : base(services) { }

				[WithAction<ActionX>]
				public partial System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Storm.Api.Dtos.Response<string>>> Do(string Name);
			}
			""";
		GeneratorDriverRunResult result = Run(src);

		string? output = result.GeneratedTrees.FirstOrDefault(t => t.FilePath.Contains("ControllerX"))?.ToString();
		output.Should().NotBeNull();
		output!.Should().Contain("Name = Name");
		output.Should().NotContain("Ignored =");
	}
}
