using System.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DependencyManager.Core.Providers;

namespace DependencyManager.Providers.Linux
{
    [Export(typeof(IPlatformProvider))]
    public class LinuxPlatformProvider : IPlatformProvider
    {
        public string Name => "Linux";

        public Task<bool> TestAsync() =>
            Task.FromResult(RuntimeInformation.IsOSPlatform(OSPlatform.Linux));

        public async Task<bool> TestAsync(string version)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
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

            if (process != null)
            {
                process.WaitForExit();
                var unameString = await process.StandardOutput.ReadToEndAsync();
                var versionPart = unameString.Substring(0, unameString.IndexOf('-'));

                var unameVersion = Version.Parse(versionPart);
                var specifiedVersion = Version.Parse(version);

                return unameVersion >= specifiedVersion;
            }

            return false;
        }
    }
}