var target = Argument("target", "Push");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    DeleteFiles("**/*.nupkg");
    CleanDirectories(GetDirectories($"./src/*/bin/{configuration}"));
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreBuild("./DependencyManager.sln", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCoreTest("./DependencyManager.sln", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
    });
});

Task("Pack")
    .IsDependentOn("Test")
    .Does(() =>
{
    DotNetCorePack("./DependencyManager.sln", new DotNetCorePackSettings
    {
        Configuration = configuration,
        NoBuild = true
    });
});

Task("Push")
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

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);