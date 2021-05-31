using DependencyManager.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager.Providers.Windows
{
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
