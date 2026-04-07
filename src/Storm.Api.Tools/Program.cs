using System.Reflection;

namespace Storm.Api.Tools;

internal class Program
{
	private const string ResourcePrefix = "Skills/";

	public static int Main(string[] args)
	{
		return args switch
		{
			["--generate-claude-skills", ..] => GenerateClaudeSkills(args),
			["--help" or "-h", ..] => ShowHelp(),
			[] => ShowHelp(),
			_ => ShowUnknownCommand(args[0]),
		};
	}

	private static int ShowHelp()
	{
		string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";

		Console.WriteLine($"""
			Storm.Api.Tools v{version}
			CLI utilities for the Storm.Api framework.

			Usage:
			  Storm.Api.Tools <command> [options]

			Commands:
			  --generate-claude-skills  Generate Claude Code skill files in .claude/skills/
			  --help, -h                Show this help message

			Options for --generate-claude-skills:
			  --output, -o <path>       Output directory (default: current directory)
			  --force                   Overwrite existing skill files without prompting

			Examples:
			  Storm.Api.Tools --generate-claude-skills
			  Storm.Api.Tools --generate-claude-skills --output ./my-project
			  Storm.Api.Tools --generate-claude-skills --force
			""");

		return 0;
	}

	private static int ShowUnknownCommand(string command)
	{
		Console.Error.WriteLine($"Unknown command: {command}");
		Console.Error.WriteLine("Run 'Storm.Api.Tools --help' for usage information.");
		return 1;
	}

	private static int GenerateClaudeSkills(string[] args)
	{
		string outputDir = ".";
		bool force = false;

		for (int i = 1; i < args.Length; i++)
		{
			switch (args[i])
			{
				case "--output" or "-o" when i + 1 < args.Length:
					outputDir = args[++i];
					break;
				case "--force":
					force = true;
					break;
				default:
					Console.Error.WriteLine($"Unknown option: {args[i]}");
					return 1;
			}
		}

		Assembly assembly = Assembly.GetExecutingAssembly();
		List<(string RelativePath, string ResourceName)> skills = assembly.GetManifestResourceNames()
			.Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal))
			.Select(name =>
			{
				string relativePath = name[ResourcePrefix.Length..].Replace('\\', '/');
				return (RelativePath: relativePath, ResourceName: name);
			})
			.OrderBy(x => x.RelativePath)
			.ToList();

		if (skills.Count == 0)
		{
			Console.Error.WriteLine("No skill files found in the tool assembly.");
			return 1;
		}

		int written = 0;
		int skipped = 0;

		foreach ((string relativePath, string resourceName) in skills)
		{
			string targetPath = Path.Combine(outputDir, ".claude", "skills", relativePath.Replace('/', Path.DirectorySeparatorChar));
			string? targetDir = Path.GetDirectoryName(targetPath);
			if (targetDir is not null)
			{
				Directory.CreateDirectory(targetDir);
			}

			if (File.Exists(targetPath) && !force)
			{
				Console.WriteLine($"  Skipped (exists): {relativePath}");
				skipped++;
				continue;
			}

			using Stream stream = assembly.GetManifestResourceStream(resourceName)!;
			using FileStream fileStream = File.Create(targetPath);
			stream.CopyTo(fileStream);

			Console.WriteLine($"  Created: {relativePath}");
			written++;
		}

		Console.WriteLine();
		Console.WriteLine($"Done. {written} file(s) written, {skipped} file(s) skipped.");

		if (skipped > 0)
		{
			Console.WriteLine("Use --force to overwrite existing files.");
		}

		return 0;
	}
}
