#load "nuget:?package=Cake.Storm.Fluent"
#load "nuget:?package=Cake.Storm.Fluent.DotNetCore"
#load "nuget:?package=Cake.Storm.Fluent.NuGet"
#load "nuget:?package=Cake.Storm.Fluent.Transformations"

const string MODULE_VERSION = "10.0.0";

Configure()
    .UseRootDirectory("..")
    .UseBuildDirectory("build")
    .UseArtifactsDirectory("artifacts")
    .AddConfiguration(c => c
        .WithSolution("Storm.Api.sln")
        .WithBuildParameter("Configuration", "Release")
        .WithBuildParameter("Platform", "Any CPU")
        .UseDefaultTooling()
        .UseDotNetCoreTooling()
        .WithDotNetCoreOutputType(OutputType.Copy)
        .UseFilesTransformation(transformation => transformation
            .OnFile("misc/Storm.Api.Dtos.nuspec")
            .OnFile("misc/Storm.Api.nuspec")
            .OnFile("misc/Storm.Api.SourceGenerators.nuspec")

            .Replace("{storm.api}", MODULE_VERSION)
        )
    )
    .AddPlatform("dotnet", c => c
        .UseCsprojTransformation(transformations => transformations.UpdatePackageVersion(MODULE_VERSION))
        .UseNugetPack(n => n
            .WithAuthor("Julien Mialon")
            .AddAllFilesFromArtifacts("lib")
			.WithDependenciesFromProject()
        )
    )
    .AddTarget("pack")
    .AddTarget("push", c => c
        .UseNugetPush(p => p.WithApiKeyFromEnvironment())
    )
    .AddApplication("dtos", c => c
        .WithProject("src/Storm.Api.Dtos/Storm.Api.Dtos.csproj")
        .WithVersion(MODULE_VERSION)
        .UseNugetPack(n => n
            .WithNuspec("misc/Storm.Api.Dtos.nuspec")
            .WithPackageId("Storm.Api.Dtos")
            .WithReleaseNotesFile("misc/Storm.Api.Dtos.md")
        )
    )
    .AddApplication("api", c => c
        .WithProject("src/Storm.Api/Storm.Api.csproj")
        .WithVersion(MODULE_VERSION)
        .UseNugetPack(n => n
            .WithNuspec("misc/Storm.Api.nuspec")
            .WithPackageId("Storm.Api")
            .WithReleaseNotesFile("misc/Storm.Api.md")
        )
    )
	.AddApplication("generators", c => c
        .WithProject("src/Storm.Api.SourceGenerators/Storm.Api.SourceGenerators.csproj")
        .WithVersion(MODULE_VERSION)
        .UseNugetPack(n => n
            .WithNuspec("misc/Storm.Api.SourceGenerators.nuspec")
            .WithPackageId("Storm.Api.SourceGenerators")
            .WithReleaseNotesFile("misc/Storm.Api.SourceGenerators.md")
        )
    )
    .Build();

RunTarget(Argument("target", "help"));