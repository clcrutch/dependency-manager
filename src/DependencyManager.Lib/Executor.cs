using DependencyManager.Core.Providers;
using DependencyManager.Providers.Default;
using DependencyManager.Providers.VSCode;
using DependencyManager.Providers.Windows;
using Microsoft.Extensions.DependencyInjection;
using DependencyManager.Providers.Linux;
using DependencyManager.Providers.Npm;

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
            services.GetService<InstallExecutor>()?.InstallAsync() ?? Task.CompletedTask;

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

            services.AddTransient<ISoftwareProvider, NpmSoftwareProvider>();
            services.AddTransient<ISoftwareProvider, VSCodeSoftwareProvider>();

            return services.BuildServiceProvider();
        }
    }
}
