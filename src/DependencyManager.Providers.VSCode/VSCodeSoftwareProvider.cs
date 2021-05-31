using DependencyManager.Core.Models;
using DependencyManager.Core.Providers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using DependencyManager.Core;

namespace DependencyManager.Providers.VSCode
{
    public class VSCodeSoftwareProvider : ISoftwareProvider
    {
        private readonly IDependencyConfigurationProvider dependencyConfigurationProvider;
        private readonly IOperatingSystemProvider operatingSystemProvider;

        public bool RequiresAdmin => false;

        public VSCodeSoftwareProvider(
            IDependencyConfigurationProvider dependencyConfigurationProvider, 
            IOperatingSystemProvider operatingSystemProvider)
        {
            this.dependencyConfigurationProvider = dependencyConfigurationProvider;
            this.operatingSystemProvider = operatingSystemProvider;
        }

        public async Task<IEnumerable<SoftwarePackage>> GetSoftwarePackagesAsync()
        {
            Dictionary<object, object> yaml = await dependencyConfigurationProvider.GetSoftwareConfigurationAsync();
            var packages = yaml["vscode"] as Dictionary<object, object>;

            return (from p in packages
                    select new SoftwarePackage(p, this, operatingSystemProvider)).ToArray();
        }

        public Task<bool> InitializationPendingAsync() =>
            Task.FromResult(false);

        public Task InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public async Task InstallPackageAsync(SoftwarePackage package)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = await operatingSystemProvider.GetFullExecutablePathAsync("code"),
                Arguments = $"--install-extension {package.PackageName}"
            });

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InstallFailedException();
            }
        }

        public async Task<bool> TestPackageInstalledAsync(SoftwarePackage package)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = await operatingSystemProvider.GetFullExecutablePathAsync("code"),
                Arguments = "--list-extensions",
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            await process.WaitForExitAsync();
            var packageString = await process.StandardOutput.ReadToEndAsync();
            var packageLines = packageString.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

            return packageLines.Contains(package.PackageName);
        }

        public async Task<bool> TestPlatformAsync() =>
            !string.IsNullOrEmpty(await operatingSystemProvider.GetFullExecutablePathAsync("code"));
    }
}
