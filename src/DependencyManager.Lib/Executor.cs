using Clcrutch.Linq;
using DependencyManager.Core;
using DependencyManager.Core.Providers;
using DependencyManager.Providers.Default;
using DependencyManager.Providers.VSCode;
using DependencyManager.Providers.Windows;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using DependencyManager.Providers.Linux;

namespace DependencyManager.Lib
{
    public class Executor
    {
        private readonly IServiceProvider services;

        public Executor()
        {
            this.services = ConfigureServices();
        }

        public Task InstallAsync() =>
            services.GetService<InstallExecutor>()?.InstallAsync();

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<IDependencyConfigurationProvider, YamlDependencyConfigurationProvider>();

            services.AddTransient<IArchitectureProvider, AllArchitectureProvider>();
            services.AddTransient<IArchitectureProvider, Amd64ArchitectureProvider>();

            services.AddTransient<IPlatformProvider, AllPlatformProvider>();
            services.AddTransient<IPlatformProvider, WindowsPlatformProvider>();
            services.AddTransient<IPlatformProvider, LinuxPlatformProvider>();

            services.AddTransient<InstallExecutor>();

            if (OperatingSystem.IsWindows())
            {
                services.AddTransient<ISoftwareProvider, ChocolateySoftwareProvider>();
                services.AddTransient<ISoftwareProvider, WindowsFeatureSoftwareProvider>();
                services.AddTransient<ISoftwareProvider, MsiSoftwareProvider>();
                services.AddTransient<ISoftwareProvider, AppxSoftwareProvider>();
                services.AddTransient<IOperatingSystemProvider, WindowsOperatingSystemProvider>();
            }
            else if (OperatingSystem.IsLinux())
            {
                services.AddTransient<ISoftwareProvider, SnapSoftwareProvider>();
                services.AddTransient<IOperatingSystemProvider, LinuxOperatingSystemProvider>();
            }

            services.AddTransient<ISoftwareProvider, VSCodeSoftwareProvider>();

            return services.BuildServiceProvider();
        }
    }
}
