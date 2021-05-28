using DependencyManager.Core;
using DependencyManager.Core.Providers;
using DependencyManager.Providers.Default;
using DependencyManager.Providers.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DependencyManager.Lib
{
    public class Executor
    {
        private readonly IServiceProvider services;

        public Executor()
        {
            this.services = ConfigureServices();
        }

        public async Task InstallAsync()
        {
            var operatingSystem = services.GetService<IOperatingSystemProvider>();
            var activeSoftwareProviders = await GetSoftwareInstallationProvidersAsync();

            foreach (var provider in activeSoftwareProviders)
            {
                if (await provider.InitializationPendingAsync())
                {
                    if (provider.RequiresAdmin && !await operatingSystem.IsUserAdminAsync())
                    {
                        throw new AdministratorRequiredException();
                    }

                    await provider.InitializeAsync();
                }

                if (await provider.ShouldInstallPackagesAsync())
                {
                    if (provider.RequiresAdmin && !await operatingSystem.IsUserAdminAsync())
                    {
                        throw new AdministratorRequiredException();
                    }

                    await provider.InstallPackagesAsync();
                }
            }
        }

        public async Task<bool> TestInstallNeededAsync()
        {
            var activeSoftwareProviders = await GetSoftwareInstallationProvidersAsync();

            foreach (var provider in activeSoftwareProviders)
            {
                if (await provider.InitializationPendingAsync() || await provider.ShouldInstallPackagesAsync())
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> RequiresAdministratorAsync()
        {

            var operatingSystem = services.GetService<IOperatingSystemProvider>();
            var activeSoftwareProviders = await GetSoftwareInstallationProvidersAsync();

            foreach (var provider in activeSoftwareProviders)
            {
                if ((await provider.InitializationPendingAsync() && provider.RequiresAdmin && !await operatingSystem.IsUserAdminAsync()) ||
                    (await provider.ShouldInstallPackagesAsync() && provider.RequiresAdmin && !await operatingSystem.IsUserAdminAsync()))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<IEnumerable<ISoftwareInstallationProvider>> GetSoftwareInstallationProvidersAsync()
        {
            var softwareInstallationProviders = services.GetServices<ISoftwareInstallationProvider>();
            var zipped = softwareInstallationProviders.Zip(await Task.WhenAll(from s in softwareInstallationProviders
                                                                              select s.CanInstallAsync()), (p, a) => (Provider: p, Activated: a));
            return from z in zipped
                   where z.Activated
                   select z.Provider;
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<IDependencyConfigurationProvider, YamlDependencyConfigurationProvider>();

            if (OperatingSystem.IsWindows())
            {
                services.AddTransient<ISoftwareInstallationProvider, ChocolateyInstallationProvider>();
                services.AddTransient<IOperatingSystemProvider, WindowsOperatingSystemProvider>();
            }

            return services.BuildServiceProvider();
        }
    }
}
