using System.Reflection;

namespace Storm.Api.Tests.Tools;

public class ProgramTests
{
	private static int InvokeMain(string[] args, out string stdout, out string stderr)
	{
		Assembly asm = typeof(Storm.Api.Dtos.Response).Assembly;
		Assembly toolsAsm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Storm.Api.Tools")
			?? Assembly.Load("Storm.Api.Tools");
		Type program = toolsAsm.GetType("Storm.Api.Tools.Program")!;
		MethodInfo main = program.GetMethod("Main", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic)!;

		TextWriter oldOut = Console.Out;
		TextWriter oldErr = Console.Error;
		using StringWriter outW = new();
		using StringWriter errW = new();
		Console.SetOut(outW);
		Console.SetError(errW);
		try
		{
			int exit = (int)main.Invoke(null, [args])!;
			stdout = outW.ToString();
			stderr = errW.ToString();
			return exit;
		}
		finally
		{
			Console.SetOut(oldOut);
			Console.SetError(oldErr);
		}
	}

	[Fact]
	public void Args_NoCommand_PrintsHelp()
	{
		int exit = InvokeMain([], out string o, out _);
		exit.Should().Be(0);
		o.Should().Contain("Storm.Api.Tools");
	}

	[Fact]
	public void Args_UnknownCommand_ReturnsNonZeroExit()
	{
		int exit = InvokeMain(["--bogus"], out _, out string e);
		exit.Should().NotBe(0);
		e.Should().Contain("Unknown command");
	}

	[Fact]
	public void GenerateClaudeSkills_DefaultOutputDirectory_WritesFiles()
	{
		string tmp = Path.Combine(Path.GetTempPath(), $"stormtool-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tmp);
		try
		{
			int exit = InvokeMain(["--generate-claude-skills", "--output", tmp], out _, out _);
			exit.Should().Be(0);
			Directory.Exists(Path.Combine(tmp, ".claude", "skills")).Should().BeTrue();
		}
		finally
		{
			try { Directory.Delete(tmp, true); } catch { }
		}
	}

	[Fact]
	public void GenerateClaudeSkills_CustomOutput_WritesToProvidedPath()
	{
		string tmp = Path.Combine(Path.GetTempPath(), $"stormtool-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tmp);
		try
		{
			InvokeMain(["--generate-claude-skills", "-o", tmp], out _, out _);
			Directory.Exists(Path.Combine(tmp, ".claude", "skills")).Should().BeTrue();
		}
		finally
		{
			try { Directory.Delete(tmp, true); } catch { }
		}
	}

	[Fact]
	public void GenerateClaudeSkills_ExistingFile_NoForce_Skipped()
	{
		string tmp = Path.Combine(Path.GetTempPath(), $"stormtool-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tmp);
		try
		{
			InvokeMain(["--generate-claude-skills", "-o", tmp], out _, out _);
			string sample = Directory.EnumerateFiles(Path.Combine(tmp, ".claude", "skills"), "*", SearchOption.AllDirectories).First();
			File.WriteAllText(sample, "MODIFIED");
			InvokeMain(["--generate-claude-skills", "-o", tmp], out string o, out _);
			File.ReadAllText(sample).Should().Be("MODIFIED");
			o.Should().Contain("Skipped");
		}
		finally
		{
			try { Directory.Delete(tmp, true); } catch { }
		}
	}

	[Fact]
	public void GenerateClaudeSkills_ExistingFile_WithForce_Overwritten()
	{
		string tmp = Path.Combine(Path.GetTempPath(), $"stormtool-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tmp);
		try
		{
			InvokeMain(["--generate-claude-skills", "-o", tmp], out _, out _);
			string sample = Directory.EnumerateFiles(Path.Combine(tmp, ".claude", "skills"), "*", SearchOption.AllDirectories).First();
			File.WriteAllText(sample, "MODIFIED");
			InvokeMain(["--generate-claude-skills", "-o", tmp, "--force"], out _, out _);
			File.ReadAllText(sample).Should().NotBe("MODIFIED");
		}
		finally
		{
			try { Directory.Delete(tmp, true); } catch { }
		}
	}

	[Fact]
	public void GenerateClaudeSkills_ExtractsEmbeddedResources()
	{
		string tmp = Path.Combine(Path.GetTempPath(), $"stormtool-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tmp);
		try
		{
			InvokeMain(["--generate-claude-skills", "-o", tmp], out _, out _);
			string skillsDir = Path.Combine(tmp, ".claude", "skills");
			Directory.EnumerateFiles(skillsDir, "*", SearchOption.AllDirectories).Should().NotBeEmpty();
		}
		finally
		{
			try { Directory.Delete(tmp, true); } catch { }
		}
	}
}
