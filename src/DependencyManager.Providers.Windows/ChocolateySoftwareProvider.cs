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
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DependencyManager.Providers.Windows
{
    public class ChocolateySoftwareProvider : SoftwareProviderBase
    {
        public override PermissionRequirements RequiredPermissions => PermissionRequirements.SuperUser;
        protected override string SectionName => "chocolatey";

        public ChocolateySoftwareProvider(
            IDependencyConfigurationProvider dependencyConfigurationProvider,
            IOperatingSystemProvider operatingSystemProvider)
            : base(dependencyConfigurationProvider, operatingSystemProvider)
        {
        }

        public override async Task InitializeAsync()
        {
            using var client = new HttpClient();
            var script = await client.GetStringAsync("https://chocolatey.org/install.ps1");

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

        public override async Task<bool> InitializationPendingAsync() =>
            string.IsNullOrEmpty(await operatingSystemProvider.GetFullExecutablePathAsync("choco.exe"));

        public override async Task InstallPackageAsync(SoftwarePackage package)
        {
            string arguments;
            if (false && await GetUpgradePending(package.PackageName))
            {
                arguments = $"upgrade {package.PackageName} -y";
            }
            else
            {
                arguments = $"install {package.PackageName} -y";
            }

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "choco",
                Arguments = arguments
            });

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InstallFailedException();
            }
        }

        public override async Task<bool> TestPackageInstalledAsync(SoftwarePackage package)
        {
            var installedPackages = await GetInstalledPackagesAsync();
            return installedPackages.ContainsKey(package.PackageName) && (true || !await GetUpgradePending(package.PackageName));
        }

        public override Task<bool> TestPlatformAsync() =>
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

            return (from l in packageLines.Skip(1).Take(index - 1)
                    where !string.IsNullOrWhiteSpace(l)
                    select l.Trim().Split(' ')).ToDictionary(s => s[0], s => s[1]);
        }

        private async Task<string> GetLatestVersionAsync(string package)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "choco",
                Arguments = $"list {package}",
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            await process.WaitForExitAsync();
            var packageString = await process.StandardOutput.ReadToEndAsync();
            var packageLines = packageString.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

            var packages = from l in packageLines.Skip(1)
                           where !string.IsNullOrWhiteSpace(l)
                           select l.Trim().Split(' ');

            var versionDict = (from p in packages
                               where p.Count() >= 2
                               select p).ToDictionary(s => s[0], s => s[1]);

            return versionDict[package];
        }

        private async Task<bool> GetUpgradePending(string package)
        {
            var installedPackages = await GetInstalledPackagesAsync();

            if (!installedPackages.ContainsKey(package))
            {
                return true;
            }

            var latestVersion = await GetLatestVersionAsync(package);
            return installedPackages[package] != latestVersion;
        }
    }
}
