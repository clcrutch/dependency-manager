using DependencyManager.Core.Providers;
using Clcrutch.Linq;
using DependencyManager.Core;
using DependencyManager.Core.Models;
using System.Composition;

namespace DependencyManager.Lib
{
    [Export]
    internal class InstallExecutor
    {
        private readonly IEnumerable<ISoftwareProvider> softwareInstallationProviders;

        public InstallExecutor(IEnumerable<ISoftwareProvider> softwareInstallationProviders)
        {
            this.softwareInstallationProviders = softwareInstallationProviders;
        }

        public async Task InstallAsync()
        {
            var softwarePackages = await softwareInstallationProviders
                                    .Select(s => s.GetSoftwarePackagesAsync())
                                    .Where(s => s != null)
                                    .SelectMany(s => s)
                                    .ToListAsync();

            var softwarePackagesByName = softwarePackages
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .ToDictionary(x => x.Name ?? string.Empty, x => x);

            foreach (var package in softwarePackages)
            {
                await InstallSoftwarePackageAsync(package, softwarePackagesByName);
            }
        }

        private async Task InstallSoftwarePackageAsync(SoftwarePackage package, Dictionary<string, SoftwarePackage> packagesByName)
        {
            if (package.Dependencies?.Any() ?? false)
            {
                var missingDependencies = (from d in package.Dependencies
                    where !packagesByName.ContainsKey(d)
                    select d).ToArray();

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

            if (!await package.TestPlatformAsync())
            {
                return;
            }

            if (!await package.TestInstalledAsync())
            {
                await package.InstallAsync();
            }
        }
    }
}
