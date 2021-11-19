using DependencyManager.Core.Models;

namespace DependencyManager.Core.Providers
{
    public enum PermissionRequirements
    {
        None,
        User,
        SuperUser
    }
    
    public interface ISoftwareProvider
    {
        PermissionRequirements RequiredPermissions { get; }

        Task InitializeAsync();
        Task<bool> InitializationPendingAsync();
        Task<IEnumerable<SoftwarePackage>> GetSoftwarePackagesAsync();
        Task InstallPackageAsync(SoftwarePackage package);
        Task<bool> TestPackageInstalledAsync(SoftwarePackage package);
        Task<bool> TestPlatformAsync();
    }
}
