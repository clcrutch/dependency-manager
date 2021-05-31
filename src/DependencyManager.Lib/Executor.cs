using DependencyManager.Core;
using DependencyManager.Core.Providers;
using DependencyManager.Providers.Default;
using DependencyManager.Providers.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;
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

            var softwareInstallationProviders = services.GetServices<ISoftwareInstallationProvider>();
            var zipped = softwareInstallationProviders.Zip(await Task.WhenAll(from s in softwareInstallationProviders
                                                                              select s.CanInstallAsync()), (p, a) => (Provider: p, Activated: a));
            var activeSoftwareProviders = from z in zipped
                                          where z.Activated
                                          select z.Provider;

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

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<IDependencyConfigurationProvider, YamlDependencyConfigurationProvider>();
            services.AddTransient<IArchitectureProvider, AllArchitectureProvider>();
            services.AddTransient<IArchitectureProvider, Amd64ArchitectureProvider>();
            services.AddTransient<IPlatformProvider, WindowsPlatformProvider>();

            if (OperatingSystem.IsWindows())
            {
                services.AddTransient<ISoftwareInstallationProvider, ChocolateyInstallationProvider>();
                services.AddTransient<IOperatingSystemProvider, WindowsOperatingSystemProvider>();
            }

            return services.BuildServiceProvider();
        }
    }
}
