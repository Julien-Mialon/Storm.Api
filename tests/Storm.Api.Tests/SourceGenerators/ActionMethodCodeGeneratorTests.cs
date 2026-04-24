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
}
