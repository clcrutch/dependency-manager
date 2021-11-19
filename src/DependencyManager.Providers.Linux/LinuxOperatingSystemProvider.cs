using System.Diagnostics;
using System.Runtime.InteropServices;
using DependencyManager.Core.Providers;

namespace DependencyManager.Providers.Linux
{
    public class LinuxOperatingSystemProvider : IOperatingSystemProvider
    {
        [DllImport("libc")]
        public static extern uint getuid();
        
        public async Task<string?> GetFullExecutablePathAsync(string executable)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-- which {executable}",
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                await process.WaitForExitAsync();
                var whichString = await process.StandardOutput.ReadToEndAsync();
                return whichString.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            }

            return null;
        }

        public Task<bool> IsSuperUserAsync() =>
            Task.FromResult(getuid() == 0);
    }
}