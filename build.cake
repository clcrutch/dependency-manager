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
    .WithCriteria(!string.IsNullOrWhiteSpace(EnvironmentVariable("NuGetApiKey")))
    .Does(() =>
{
    var nugetApiKey = EnvironmentVariable("NuGetApiKey");

    NuGetPush(GetFiles("**/Clcrutch.DependencyManager.*.nupkg"), new NuGetPushSettings
    {
        ApiKey = nugetApiKey
    });
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);