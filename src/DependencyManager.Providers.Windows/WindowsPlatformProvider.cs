using DependencyManager.Core.Providers;
using System.Composition;

namespace DependencyManager.Providers.Windows
{
    [Export(typeof(IPlatformProvider))]
    public class WindowsPlatformProvider : IPlatformProvider
    {
        public string Name => "Windows";

        public Task<bool> TestAsync() =>
            Task.FromResult(OperatingSystem.IsWindows());

        public Task<bool> TestAsync(string version)
        {
            var versionObj = Version.Parse(version);
            return Task.FromResult(OperatingSystem.IsWindowsVersionAtLeast(versionObj.Major, versionObj.Minor, versionObj.Build, versionObj.Revision));
        }
    }
}
