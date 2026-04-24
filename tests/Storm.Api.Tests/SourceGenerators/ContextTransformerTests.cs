using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Storm.Api.SourceGenerators.ActionMethods;

namespace Storm.Api.Tests.SourceGenerators;

// Exercises the transform via the generator pipeline, rather than directly instantiating the internal ContextTransformer.
public class ContextTransformerTests
{
	private static MetadataReference[] References()
	{
		_ = typeof(Storm.Api.Unit);
		_ = typeof(Storm.Api.Dtos.Response);
		_ = typeof(Storm.Api.CQRS.IAction<,>);
		_ = typeof(Storm.Api.CQRS.Domains.Results.ApiFileResult);
		_ = typeof(Microsoft.AspNetCore.Mvc.FileResult);
		_ = typeof(Microsoft.AspNetCore.Mvc.ControllerBase);
		_ = typeof(Storm.Api.Controllers.BaseController);

		string trusted = (string)AppDomain.CurrentDomain.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
		HashSet<string> all = [.. trusted.Split(Path.PathSeparator)];
		foreach (System.Reflection.Assembly a in AppDomain.CurrentDomain.GetAssemblies())
		{
			if (!a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location))
			{
				all.Add(a.Location);
			}
		}
		return all.Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => (MetadataReference)MetadataReference.CreateFromFile(p)).ToArray();
	}

	private static GeneratorDriverRunResult Run(string source, MetadataReference[]? refs = null)
	{
		CSharpCompilation compilation = CSharpCompilation.Create(
			"T",
			[CSharpSyntaxTree.ParseText(source)],
			refs ?? References(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		GeneratorDriver driver = CSharpGeneratorDriver.Create(new ActionMethodCodeGenerator());
		return driver.RunGenerators(compilation).GetRunResult();
	}

	private const string Base = """
		using Storm.Api;
		using Storm.Api.CQRS;
		using Storm.Api.Dtos;
		using Storm.Api.CQRS.Domains.Results;
		using Storm.Api.SourceGenerators.ActionMethods;
		namespace T;
		""";

	[Fact]
	public void CreateContext_ResolvesAllRequiredTypes()
	{
		string src = Base + """
			public class P {}
			public class A : IAction<P, int> { public System.Threading.Tasks.Task<int> Execute(P x) => System.Threading.Tasks.Task.FromResult(0); }
			public partial class C : Storm.Api.Controllers.BaseController { public C(System.IServiceProvider s):base(s){} [WithAction<A>] public partial System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Response<int>>> Do(); }
			""";
		Run(src).GeneratedTrees.Should().Contain(t => t.FilePath.Contains("C.Storm.Api.ActionMethods"));
	}

	[Fact]
	public void CreateContext_DetectsActionType_Unit()
	{
		string src = Base + """
			public class P {}
			public class A : IAction<P, Unit> { public System.Threading.Tasks.Task<Unit> Execute(P x) => System.Threading.Tasks.Task.FromResult(Unit.Default); }
			public partial class C : Storm.Api.Controllers.BaseController { public C(System.IServiceProvider s):base(s){} [WithAction<A>] public partial System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Response>> Do(); }
			""";
		string output = Run(src).GeneratedTrees.First(t => t.FilePath.Contains("C.Storm.Api.ActionMethods")).ToString();
		output.Should().Contain("IsSuccess = true");
		output.Should().NotContain("Data = actionResult");
	}

	[Fact]
	public void CreateContext_DetectsActionType_Response()
	{
		string src = Base + """
			public class P {}
			public class A : IAction<P, Response<string>> { public System.Threading.Tasks.Task<Response<string>> Execute(P x) => System.Threading.Tasks.Task.FromResult(new Response<string>()); }
			public partial class C : Storm.Api.Controllers.BaseController { public C(System.IServiceProvider s):base(s){} [WithAction<A>] public partial System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Response<string>>> Do(); }
			""";
		string output = Run(src).GeneratedTrees.First(t => t.FilePath.Contains("C.Storm.Api.ActionMethods")).ToString();
		output.Should().Contain("return await Services.ExecuteAction<");
	}

	[Fact]
	public void CreateContext_DetectsActionType_ApiFileResult()
	{
		string src = Base + """
			public class P {}
			public class A : IAction<P, ApiFileResult> { public System.Threading.Tasks.Task<ApiFileResult> Execute(P x) => System.Threading.Tasks.Task.FromResult(ApiFileResult.Create(new byte[]{1},"x")); }
			public partial class C : Storm.Api.Controllers.BaseController { public C(System.IServiceProvider s):base(s){} [WithAction<A>] public partial System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.IActionResult> Do(); }
			""";
		string output = Run(src).GeneratedTrees.First(t => t.FilePath.Contains("C.Storm.Api.ActionMethods")).ToString();
		output.Should().Contain("FileAction<");
	}

	[Fact]
	public void CreateContext_DetectsActionType_RegularT()
	{
		string src = Base + """
			public class P {}
			public class A : IAction<P, int> { public System.Threading.Tasks.Task<int> Execute(P x) => System.Threading.Tasks.Task.FromResult(0); }
			public partial class C : Storm.Api.Controllers.BaseController { public C(System.IServiceProvider s):base(s){} [WithAction<A>] public partial System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Response<int>>> Do(); }
			""";
		string output = Run(src).GeneratedTrees.First(t => t.FilePath.Contains("C.Storm.Api.ActionMethods")).ToString();
		output.Should().Contain("Data = actionResult");
	}

	[Fact]
	public void CreateContext_ExtractsActionTypeFromWithActionGeneric()
	{
		string src = Base + """
			public class P {}
			public class MyAction : IAction<P, int> { public System.Threading.Tasks.Task<int> Execute(P x) => System.Threading.Tasks.Task.FromResult(0); }
			public partial class C : Storm.Api.Controllers.BaseController { public C(System.IServiceProvider s):base(s){} [WithAction<MyAction>] public partial System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Response<int>>> Do(); }
			""";
		string output = Run(src).GeneratedTrees.First(t => t.FilePath.Contains("C.Storm.Api.ActionMethods")).ToString();
		output.Should().Contain("MyAction");
	}

	[Fact]
	public void CreateContext_ParameterWithMapTo_MapsExplicitly()
	{
		string src = Base + """
			public class P { public string Name { get; set; } = ""; }
			public class A : IAction<P, int> { public System.Threading.Tasks.Task<int> Execute(P x) => System.Threading.Tasks.Task.FromResult(0); }
			public partial class C : Storm.Api.Controllers.BaseController { public C(System.IServiceProvider s):base(s){} [WithAction<A>] public partial System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Response<int>>> Do([MapTo("Name")] string anything); }
			""";
		string output = Run(src).GeneratedTrees.First(t => t.FilePath.Contains("C.Storm.Api.ActionMethods")).ToString();
		output.Should().Contain("Name = anything");
	}

	[Fact]
	public void CreateContext_ParameterWithoutMapTo_AutoMapsByNameAndType()
	{
		string src = Base + """
			public class P { public string Name { get; set; } = ""; }
			public class A : IAction<P, int> { public System.Threading.Tasks.Task<int> Execute(P x) => System.Threading.Tasks.Task.FromResult(0); }
			public partial class C : Storm.Api.Controllers.BaseController { public C(System.IServiceProvider s):base(s){} [WithAction<A>] public partial System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Response<int>>> Do(string Name); }
			""";
		string output = Run(src).GeneratedTrees.First(t => t.FilePath.Contains("C.Storm.Api.ActionMethods")).ToString();
		output.Should().Contain("Name = Name");
	}

	[Fact]
	public void CreateContext_MultipleParametersSameType_NameMatchPrioritized()
	{
		string src = Base + """
			public class P { public string First { get; set; } = ""; public string Second { get; set; } = ""; }
			public class A : IAction<P, int> { public System.Threading.Tasks.Task<int> Execute(P x) => System.Threading.Tasks.Task.FromResult(0); }
			public partial class C : Storm.Api.Controllers.BaseController { public C(System.IServiceProvider s):base(s){} [WithAction<A>] public partial System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Response<int>>> Do(string First, string Second); }
			""";
		string output = Run(src).GeneratedTrees.First(t => t.FilePath.Contains("C.Storm.Api.ActionMethods")).ToString();
		output.Should().Contain("First = First");
		output.Should().Contain("Second = Second");
	}

	[Fact]
	public void CreateContext_MethodWithNoParameters_UsesDefaultConstructor()
	{
		string src = Base + """
			public class P {}
			public class A : IAction<P, int> { public System.Threading.Tasks.Task<int> Execute(P x) => System.Threading.Tasks.Task.FromResult(0); }
			public partial class C : Storm.Api.Controllers.BaseController { public C(System.IServiceProvider s):base(s){} [WithAction<A>] public partial System.Threading.Tasks.Task<Microsoft.AspNetCore.Mvc.ActionResult<Response<int>>> Do(); }
			""";
		string output = Run(src).GeneratedTrees.First(t => t.FilePath.Contains("C.Storm.Api.ActionMethods")).ToString();
		output.Should().Contain("new()");
	}

	[Fact]
	public void CreateContext_SkipsMethodsWithoutWithActionAttribute()
	{
		string src = Base + """
			public class P {}
			public partial class C : Storm.Api.Controllers.BaseController { public C(System.IServiceProvider s):base(s){} public partial void Noop(); public partial void Noop() { } }
			""";
		Run(src).GeneratedTrees.Should().NotContain(t => t.FilePath.Contains("C.Storm.Api.ActionMethods"));
	}
}
