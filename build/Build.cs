using System.Collections.Generic;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Semver;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Version number that is built.")]
    readonly string Version = "0.0.0";

    [Solution] readonly Solution Solution;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    IEnumerable<string> Runtimes = new[] { "linux-x64", "linux-musl-x64", "osx-x64" };

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            DotNetClean(s => s.SetProject(Solution));
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() => DotNetBuild(s => s
            .SetProjectFile(Solution)
            .SetConfiguration(Configuration)
            .EnableNoRestore())
        );

    Target Publish => _ => _
        .DependsOn(Clean, Compile)
        .Executes(() => DotNetPublish(s => s
            .SetProject(Solution)
            .SetConfiguration(Configuration)
            .AddProperty("PublishSingleFile", true)
            .SetVersion(Version)
            .SetAssemblyVersion(Version)
            .SetFileVersion(Version)
            .SetInformationalVersion(Version)
            .CombineWith(
                Runtimes,
                (settings, runtime) => settings
                    .SetRuntime(runtime)
                    .SetOutput(ArtifactsDirectory / runtime))));

    Target Ci => _ => _
        .OnlyWhenStatic(() => Version != "0.0.0" && SemVersion.Parse(Version, true) != null)
        .DependsOn(Publish);
}
