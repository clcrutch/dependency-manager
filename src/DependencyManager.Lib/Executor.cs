using DependencyManager.Providers.Default;
using DependencyManager.Providers.VSCode;
using DependencyManager.Providers.Windows;
using Microsoft.Extensions.DependencyInjection;
using DependencyManager.Providers.Linux;
using DependencyManager.Providers.Npm;
using Clcrutch.Extensions.DependencyInjection.Catalogs;
using DependencyManager.Providers.DotNet;

namespace DependencyManager.Lib
{
    public class Executor
    {
        private readonly SemaphoreSlim servicesSemaphore = new(1);
        private IServiceProvider? services;

        public async Task InstallAsync()
        {
            var services = await GetServiceProviderAsync();
            await (services.GetService<InstallExecutor>()?.InstallAsync() ?? Task.CompletedTask);
        }

        public async Task<bool> TestAsync()
        {
            var services = await GetServiceProviderAsync();
            return await (services.GetService<TestExecutor>()?.TestAsync() ?? Task.FromResult(false));
        }

        private async Task<IServiceProvider> GetServiceProviderAsync()
        {
            await servicesSemaphore.WaitAsync();
            if (services == null)
            {
                services = await ConfigureServicesAsync();
            }
            servicesSemaphore.Release();

            return services;
        }

        private Task<IServiceProvider> ConfigureServicesAsync()
        {
            var services = new ServiceCollection();

            var catalog = new AggregateCatalog(
                services,
                new AssemblyCatalog(typeof(InstallExecutor).Assembly, services),
                new AssemblyCatalog(typeof(AllArchitectureProvider).Assembly, services),
                new AssemblyCatalog(typeof(WindowsOperatingSystemProvider).Assembly, services),
                new AssemblyCatalog(typeof(LinuxOperatingSystemProvider).Assembly, services),
                new AssemblyCatalog(typeof(NpmSoftwareProvider).Assembly, services),
                new AssemblyCatalog(typeof(VSCodeSoftwareProvider).Assembly, services),
                new AssemblyCatalog(typeof(DotNetSoftwareProvider).Assembly, services)
            );

            return catalog.GetServiceProvider();
        }
    }
}
