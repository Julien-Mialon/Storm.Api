#load "nuget:?package=Cake.Storm.Fluent"
#load "nuget:?package=Cake.Storm.Fluent.DotNetCore"
#load "nuget:?package=Cake.Storm.Fluent.NuGet"
#load "nuget:?package=Cake.Storm.Fluent.Transformations"

const string MODULE_VERSION = "0.4.3";

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
            .OnFile("misc/Storm.Api.Core.nuspec")
            .OnFile("misc/Storm.Api.Dtos.nuspec")
            .OnFile("misc/Storm.Api.nuspec")
            .OnFile("misc/Storm.SqlMigrations.nuspec")

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
    .AddApplication("core", c => c
        .WithProject("src/Storm.Api.Core/Storm.Api.Core.csproj")
        .WithVersion(MODULE_VERSION)
        .UseNugetPack(n => n
            .WithNuspec("misc/Storm.Api.Core.nuspec")
            .WithPackageId("Storm.Api.Core")
            .WithReleaseNotesFile("misc/Storm.Api.Core.md")
        )
    )
    .AddApplication("migrations", c => c
        .WithProject("src/Storm.SqlMigrations/Storm.SqlMigrations.csproj")
        .WithVersion(MODULE_VERSION)
        .UseNugetPack(n => n
            .WithNuspec("misc/Storm.SqlMigrations.nuspec")
            .WithPackageId("Storm.SqlMigrations")
            .WithReleaseNotesFile("misc/Storm.SqlMigrations.md")
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
    .Build();

RunTarget(Argument("target", "help"));