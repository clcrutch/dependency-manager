using DependencyManager.Core.Providers;
using System.Composition;
using System.Runtime.InteropServices;

namespace DependencyManager.Providers.Windows
{
    [Export(typeof(IPlatformProvider))]
    public class WindowsPlatformProvider : IPlatformProvider
    {
        public string Name => "Windows";

        public Task<bool> TestAsync() =>
            Task.FromResult(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

        public async Task<bool> TestAsync(string version)
        {
            var versionObj = Version.Parse(version);
            return (await TestAsync()) && Environment.OSVersion.Version >= versionObj;
        }
    }
}
