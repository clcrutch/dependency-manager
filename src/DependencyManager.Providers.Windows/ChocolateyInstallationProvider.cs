using DependencyManager.Core;
using DependencyManager.Core.Providers;
using Microsoft.PowerShell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Net;
using System.Threading.Tasks;

namespace DependencyManager.Providers.Windows
{
    public class ChocolateyInstallationProvider : ISoftwareInstallationProvider
    {
        private readonly IDependencyConfigurationProvider dependencyConfigurationProvider;

        public bool RequiresAdmin => true;

        public ChocolateyInstallationProvider(IDependencyConfigurationProvider dependencyConfigurationProvider)
        {
            this.dependencyConfigurationProvider = dependencyConfigurationProvider;
        }

        public async Task<bool> CanInstallAsync()
        {
            Dictionary<object, object> yaml = await dependencyConfigurationProvider.GetSoftwareConfigurationAsync();
            if (!yaml.ContainsKey("windows"))
            {
                return false;
            }

            Dictionary<object, object> windows = yaml["windows"] as Dictionary<object, object>;
            return windows.ContainsKey("chocolatey");
        }

        public async Task InitializeAsync()
        {
            using var client = new WebClient();
            var script = await client.DownloadStringTaskAsync("https://chocolatey.org/install.ps1");

            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.ExecutionPolicy = ExecutionPolicy.Unrestricted;

            using var powershell = PowerShell.Create(initialSessionState);
            powershell.AddScript(script);
            await powershell.InvokeAsync();

            Environment.SetEnvironmentVariable("PATH",
                $"{Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine)};" +
                $"{Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.User)};" +
                $"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "chocolatey", "bin")}");
        }

        public Task<bool> InitializationPendingAsync() =>
            Task.FromResult(!Environment
                .GetEnvironmentVariable("PATH")
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Any(p => File.Exists(Path.Combine(p, "choco.exe"))));

        public async Task InstallPackagesAsync()
        {
            var packagesToInstall = await GetPackagesToInstallAsync();

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "choco",
                Arguments = $"install {string.Join(' ', packagesToInstall)} -y"
            });

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InstallFailedException();
            }
        }

        public async Task<bool> ShouldInstallPackagesAsync() =>
            (await GetPackagesToInstallAsync()).Any();

        private async Task<IEnumerable<string>> GetPackagesToInstallAsync()
        {
            dynamic yaml = await dependencyConfigurationProvider.GetSoftwareConfigurationAsync();
            var packages = await GetInstalledPackagesAsync();

            Dictionary<object, object> chocoPackages = yaml["windows"]["chocolatey"];

            return chocoPackages.Keys.Select(x => x as string).Except(packages.Keys);
        }

        private async Task<Dictionary<string, string>> GetInstalledPackagesAsync()
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "choco",
                Arguments = "list --local-only",
                RedirectStandardOutput = true
            });

            await process.WaitForExitAsync();
            var packageString = await process.StandardOutput.ReadToEndAsync();
            var packageLines = packageString.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var firstLine = packageLines.First();
            var lastLine = packageLines.Last();

            return (from l in packageLines
                    where l != firstLine && l != lastLine
                    select l.Split(' ')).ToDictionary(s => s[0], s => s[1]);
        }
    }
}
