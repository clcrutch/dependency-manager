using Clcrutch.Extensions.DependencyInjection;
using DependencyManager.Core;
using DependencyManager.Core.Models;
using DependencyManager.Core.Providers;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DependencyManager.Providers.Windows
{
    // [Export(typeof(ISoftwareProvider))]
    // [OperatingSystemRequired(OperatingSystems.Windows)]
    public class WingetSoftwareProvider : SoftwareProviderBase
    {
        public override PermissionRequirements RequiredPermissions => PermissionRequirements.User;

        protected override string SectionName => "winget";

        public WingetSoftwareProvider(
            IDependencyConfigurationProvider dependencyConfigurationProvider,
            IOperatingSystemProvider operatingSystemProvider)
            : base(dependencyConfigurationProvider, operatingSystemProvider)
        {
        }


        public override Task<bool> InitializationPendingAsync() =>
            Task.FromResult(false);

        public override Task InitializeAsync() =>
            Task.CompletedTask;

        public override async Task InstallPackageAsync(SoftwarePackage package)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "winget",
                Arguments = $"install {package.Id} --accept-package-agreements --accept-source-agreements"
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
            return installedPackages?.Contains(package.Id) ?? false;
        }

        public override Task<bool> TestPlatformAsync() =>
            Task.FromResult(OperatingSystem.IsWindows());

        private async Task<IEnumerable<string>?> GetInstalledPackagesAsync()
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "winget",
                Arguments = "list",
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                await process.WaitForExitAsync();
                var packageString = await process.StandardOutput.ReadToEndAsync();
                var packageLines = packageString.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

                return from l in packageLines.Skip(2)
                       where !string.IsNullOrWhiteSpace(l)
                       select l.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
            }

            return null;
        }
    }
}
