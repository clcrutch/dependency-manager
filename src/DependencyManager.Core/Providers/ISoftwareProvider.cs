using DependencyManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager.Core.Providers
{
    public interface ISoftwareProvider
    {
        bool InstallRequiresAdmin { get; }

        Task InitializeAsync();
        Task<bool> InitializationPendingAsync();
        Task<IEnumerable<SoftwarePackage>> GetSoftwarePackagesAsync();
        Task InstallPackageAsync(SoftwarePackage package);
        Task<bool> TestPackageInstalledAsync(SoftwarePackage package);
        Task<bool> TestPlatformAsync();
    }
}
