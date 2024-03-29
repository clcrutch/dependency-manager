﻿using Clcrutch.Extensions.DependencyInjection;
using Clcrutch.Linq;
using DependencyManager.Core;
using DependencyManager.Core.Models;
using DependencyManager.Core.Providers;
using Microsoft.PowerShell;
using System.Composition;
using System.Diagnostics;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text.RegularExpressions;

namespace DependencyManager.Providers.Windows
{
    [Export(typeof(ISoftwareProvider))]
    [OperatingSystemRequired(OperatingSystems.Windows)]
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

            if (operatingSystemProvider is WindowsOperatingSystemProvider windowsOperatingSystemProvider)
            {
                windowsOperatingSystemProvider.UpdatePathEnvironmentVariable();
            }
        }

        public override async Task<bool> InitializationPendingAsync() =>
            string.IsNullOrEmpty(await operatingSystemProvider.GetFullExecutablePathAsync("choco.exe"));

        public override async Task InstallPackageAsync(SoftwarePackage package)
        {
            string arguments;
            if (await GetUpgradePending(package.PackageName))
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

            await (process?.WaitForExitAsync() ?? Task.CompletedTask);

            if (process == null || process.ExitCode != 0)
            {
                throw new InstallFailedException();
            }
        }

        public override async Task<bool> TestPackageInstalledAsync(SoftwarePackage package)
        {
            var installedPackages = await GetInstalledPackagesAsync();
            return (installedPackages?.ContainsKey(package.PackageName) ?? false) && (true || !await GetUpgradePending(package.PackageName));
        }

        public override Task<bool> TestPlatformAsync() =>
            Task.FromResult(OperatingSystem.IsWindows());

        private async Task<Dictionary<string, string>?> GetInstalledPackagesAsync()
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "choco",
                Arguments = "list --local-only",
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
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

            return null;
        }

        private async Task<string?> GetLatestVersionAsync(string package)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "choco",
                Arguments = $"list {package}",
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                await process.WaitForExitAsync();
                var packageString = await process.StandardOutput.ReadToEndAsync();
                var packageLines = packageString.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

                var packages = from l in packageLines.Skip(1)
                               where !string.IsNullOrWhiteSpace(l)
                               select l.Trim().Split(' ');

                var versionDict = (from p in packages
                                   where p.Length >= 2
                                   select p).ToDictionary(s => s[0], s => s[1]);

                return versionDict[package];
            }

            return null;
        }

        private async Task<bool> GetUpgradePending(string package)
        {
            var installedPackages = await GetInstalledPackagesAsync();

            if (installedPackages == null)
            {
                return false;
            }

            if (!installedPackages.ContainsKey(package))
            {
                return false;
            }

            var latestVersion = await GetLatestVersionAsync(package);
            return installedPackages[package] != latestVersion;
        }
    }
}
