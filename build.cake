#addin Cake.GitVersioning
#addin "Cake.Git"

var target = Argument("target", "Push");
var configuration = Argument("configuration", "Release");

var operatingSystems = new List<string>()
{
    "win7-x64"
};

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    DeleteFiles("**/*.nupkg");
    DeleteFiles("*.zip");
    CleanDirectories(GetDirectories($"./src/*/bin/{configuration}"));
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetBuild("./DependencyManager.sln", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetTest("./DependencyManager.sln", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
    });
});

Task("Publish")
    .IsDependentOn("Test")
    .Does(() =>
{
    foreach (var operatingSystem in operatingSystems)
    {
        DotNetPublish("./DependencyManager.sln", new DotNetPublishSettings
        {
            Configuration = configuration,
            SelfContained = true,
            Runtime = operatingSystem
        });

        Zip($"src/DependencyManager/bin/Release/net6.0/{operatingSystem}/publish", $"./{operatingSystem}.zip");
    }
});

Task("Pack")
    .IsDependentOn("Test")
    .Does(() =>
{
    DotNetPack("./DependencyManager.sln", new DotNetCorePackSettings
    {
        Configuration = configuration,
        NoBuild = true
    });
});

Task("GitHub-Push")
    .IsDependentOn("Publish")
    .WithCriteria(!string.IsNullOrWhiteSpace(EnvironmentVariable("GITHUB_TOKEN")))
    .Does(() =>
{
    var versionTag = $"v{GitVersioningGetVersion().SemVer2}";
    var message = "Test Message"; //GitLogTip("./").MessageShort;
    
    StartProcess("gh", new ProcessSettings {
        Arguments = new ProcessArgumentBuilder()
            .Append("release")
            .Append("create")
            .Append(versionTag)
            .Append("-p")
            .Append("-n")
            .Append($"\"{message}\"")
    });

    foreach (var operatingSystem in operatingSystems)
    {
        StartProcess("gh", new ProcessSettings {
            Arguments = new ProcessArgumentBuilder()
                .Append("release")
                .Append("upload")
                .Append(versionTag)
                .Append($"./{operatingSystem}.zip")
        });
    }
});

Task("NuGet-Push")
    .IsDependentOn("Pack")
    .WithCriteria(!string.IsNullOrWhiteSpace(EnvironmentVariable("NUGET_API_KEY")))
    .Does(() =>
{
    var nugetApiKey = EnvironmentVariable("NUGET_API_KEY");

    NuGetPush(GetFiles("**/Clcrutch.DependencyManager.*.nupkg"), new NuGetPushSettings
    {
        ApiKey = nugetApiKey,
        Source = "https://api.nuget.org/v3/index.json"
    });
});

Task("Push")
    .IsDependentOn("GitHub-Push")
    .IsDependentOn("NuGet-Push");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);