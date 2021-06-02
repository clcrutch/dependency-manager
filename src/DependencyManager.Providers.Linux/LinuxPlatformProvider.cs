using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DependencyManager.Core.Providers;

namespace DependencyManager.Providers.Linux
{
    public class LinuxPlatformProvider : IPlatformProvider
    {
        public string Name => "Linux";

        public Task<bool> TestAsync() =>
            Task.FromResult(OperatingSystem.IsLinux());

        public async Task<bool> TestAsync(string version)
        {
            if (!OperatingSystem.IsLinux())
            {
                return false;
            }
            
            
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "uname",
                Arguments = "-r",
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            await process.WaitForExitAsync();
            var unameString = await process.StandardOutput.ReadToEndAsync();
            var versionPart = unameString.Substring(0, unameString.IndexOf('-'));

            var unameVersion = Version.Parse(versionPart);
            var specifiedVersion = Version.Parse(version);

            return unameVersion >= specifiedVersion;
        }
    }
}