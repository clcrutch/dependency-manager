using DependencyManager.Core.Providers;
using System;
using System.Collections.Generic;
using Clcrutch.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyManager.Core;
using DependencyManager.Core.Models;

namespace DependencyManager.Lib
{
    class InstallExecutor
    {
        private readonly IOperatingSystemProvider operatingSystem;
        private readonly IEnumerable<ISoftwareProvider> softwareInstallationProviders;

        public InstallExecutor(IOperatingSystemProvider operatingSystem, IEnumerable<ISoftwareProvider> softwareInstallationProviders)
        {
            this.operatingSystem = operatingSystem;
            this.softwareInstallationProviders = softwareInstallationProviders;
        }

        public async Task InstallAsync()
        {
            var softwarePackages = await softwareInstallationProviders
                                    .Where(s => s.TestPlatformAsync())
                                    .Select(s => s.GetSoftwarePackagesAsync())
                                    .SelectMany(s => s)
                                    .ToListAsync();

            var softwarePackagesByName = softwarePackages
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .ToDictionary(x => x.Name, x => x);

            foreach (var package in softwarePackages)
            {
                await InstallSoftwarePackageAsync(package, softwarePackagesByName);
            }
        }

        private async Task InstallSoftwarePackageAsync(SoftwarePackage package, Dictionary<string, SoftwarePackage> packagesByName)
        {
            if (package.Dependencies?.Any() ?? false)
            {
                var missingDependencies = from d in package.Dependencies
                                          where !packagesByName.ContainsKey(d)
                                          select d;

                if (missingDependencies.Any())
                {
                    throw new DependencyMissingException(missingDependencies.ToArray());
                }

                var dependencies = from d in package.Dependencies
                                   select packagesByName[d];

                foreach (var depend in dependencies)
                {
                    await InstallSoftwarePackageAsync(depend, packagesByName);
                }
            }
            
            if (!await package.TestInstalledAsync())
            {
                await package.InstallAsync();
            }
        }
    }
}
