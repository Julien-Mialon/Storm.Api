using System.Runtime.CompilerServices;
using Xunit;

namespace Storm.Api.SourceGenerators.Tests.Infrastructure;

/// <summary>
/// Golden-file (snapshot) assertions: a test compares the <em>entire</em> generated controller against
/// a committed expected file under <c>Snapshots/</c>, rather than probing it with substring checks.
/// <para>
/// Snapshots are keyed by the calling test class + method name. Set the environment variable
/// <c>UPDATE_SNAPSHOTS=1</c> to (re)write the expected files after an intentional generator change;
/// review the diff and commit. A missing snapshot is created on first run.
/// </para>
/// </summary>
internal static class Snapshot
{
	private static readonly bool UPDATE = string.Equals(Environment.GetEnvironmentVariable("UPDATE_SNAPSHOTS"), "1", StringComparison.Ordinal)
		|| string.Equals(Environment.GetEnvironmentVariable("UPDATE_SNAPSHOTS"), "true", StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// Asserts that the full generated source for <paramref name="controllerClassName"/> matches its
	/// committed snapshot. When <paramref name="assertCompiles"/> is true (the default) the run must
	/// also be free of generator diagnostics and the generated code must compile.
	/// </summary>
	public static void MatchController(
		GeneratorOutput output,
		string controllerClassName,
		bool assertCompiles = true,
		[CallerFilePath] string callerFilePath = "",
		[CallerMemberName] string testName = "")
	{
		if (assertCompiles)
		{
			Assert.Empty(output.GeneratorDiagnostics);
			Assert.Empty(output.CompilationErrors);
		}

		string actual = Normalize(output.GeneratedControllerFor(controllerClassName));

		string directory = Path.Combine(Path.GetDirectoryName(callerFilePath)!, "Snapshots");
		string testClassName = Path.GetFileNameWithoutExtension(callerFilePath);
		string path = Path.Combine(directory, $"{testClassName}.{testName}.verified.cs");

		if (UPDATE || File.Exists(path) is false)
		{
			Directory.CreateDirectory(directory);
			File.WriteAllText(path, actual);
		}

		string expected = Normalize(File.ReadAllText(path));
		Assert.Equal(expected, actual);
	}

	private static string Normalize(string text) => text.Replace("\r\n", "\n").Replace("\r", "\n");
}
