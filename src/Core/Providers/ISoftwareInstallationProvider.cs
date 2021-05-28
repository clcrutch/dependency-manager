using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager.Core.Providers
{
    public interface ISoftwareInstallationProvider
    {
        bool RequiresAdmin { get; }

        Task<bool> CanInstallAsync();
        Task InitializeAsync();
        Task<bool> InitializationPendingAsync();
        Task InstallPackagesAsync();
        Task<bool> ShouldInstallPackagesAsync();
    }
}
