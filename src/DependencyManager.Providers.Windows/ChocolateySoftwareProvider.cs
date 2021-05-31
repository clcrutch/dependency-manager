using Clcrutch.Linq;
using DependencyManager.Core;
using DependencyManager.Core.Models;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DependencyManager.Providers.Windows
{
    public class ChocolateySoftwareProvider : ISoftwareProvider
    {
        private readonly IDependencyConfigurationProvider dependencyConfigurationProvider;
        private readonly IOperatingSystemProvider operatingSystemProvider;

        public bool RequiresAdmin => true;

        public ChocolateySoftwareProvider(
            IDependencyConfigurationProvider dependencyConfigurationProvider,
            IOperatingSystemProvider operatingSystemProvider)
        {
            this.dependencyConfigurationProvider = dependencyConfigurationProvider;
            this.operatingSystemProvider = operatingSystemProvider;
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

        public async Task<IEnumerable<SoftwarePackage>> GetSoftwarePackagesAsync()
        {
            Dictionary<object, object> yaml = await dependencyConfigurationProvider.GetSoftwareConfigurationAsync();
            var packages = yaml["chocolatey"] as Dictionary<object, object>;

            return (from p in packages
                    select new SoftwarePackage(p, this, operatingSystemProvider)).ToArray();
        }

        public async Task InstallPackageAsync(SoftwarePackage package)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "choco",
                Arguments = $"install {package.PackageName} -y"
            });

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InstallFailedException();
            }
        }

        public async Task<bool> TestPackageInstalledAsync(SoftwarePackage package)
        {
            var installedPackages = await GetInstalledPackagesAsync();
            return installedPackages.ContainsKey(package.PackageName);
        }

        public Task<bool> TestPlatformAsync() =>
            Task.FromResult(OperatingSystem.IsWindows());

        private async Task<Dictionary<string, string>> GetInstalledPackagesAsync()
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "choco",
                Arguments = "list --local-only",
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            await process.WaitForExitAsync();
            var packageString = await process.StandardOutput.ReadToEndAsync();
            var packageLines = packageString.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

            var index = packageLines.IndexOf((from l in packageLines
                                              where Regex.Match(l, "[0-9]+ packages installed.").Success
                                              select l).Single());

            var firstLine = packageLines.First();
            var lastLine = packageLines.Last();

            return (from l in packageLines.Skip(1).Take(index - 1)
                    select l.Split(' ')).ToDictionary(s => s[0], s => s[1]);
        }
    }
}
