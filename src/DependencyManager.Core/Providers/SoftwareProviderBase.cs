using DependencyManager.Core.Models;

namespace DependencyManager.Core.Providers
{
    public abstract class SoftwareProviderBase : ISoftwareProvider
    {
        protected readonly IDependencyConfigurationProvider dependencyConfigurationProvider;
        protected readonly IOperatingSystemProvider operatingSystemProvider;

        public abstract PermissionRequirements RequiredPermissions { get; }
        protected abstract string SectionName { get; }

        protected SoftwareProviderBase(
            IDependencyConfigurationProvider dependencyConfigurationProvider,
            IOperatingSystemProvider operatingSystemProvider)
        {
            this.dependencyConfigurationProvider = dependencyConfigurationProvider;
            this.operatingSystemProvider = operatingSystemProvider;
        }

        public virtual async Task<IEnumerable<SoftwarePackage>> GetSoftwarePackagesAsync()
        {
            var yaml = await dependencyConfigurationProvider.GetSoftwareConfigurationAsync();
            if (!yaml.ContainsKey(SectionName))
            {
                return Enumerable.Empty<SoftwarePackage>();
            }

            var packages = yaml[SectionName] as Dictionary<object, object>;

            return (from p in packages
                    select new SoftwarePackage(p, this, operatingSystemProvider)).ToArray();
        }

        public abstract Task<bool> InitializationPendingAsync();
        public abstract Task InitializeAsync();
        public abstract Task InstallPackageAsync(SoftwarePackage package);
        public abstract Task<bool> TestPackageInstalledAsync(SoftwarePackage package);
        public abstract Task<bool> TestPlatformAsync();
    }
}
