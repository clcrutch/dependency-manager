using Clcrutch.Linq;
using DependencyManager.Core.Providers;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager.Lib
{
    [Export]
    internal class TestExecutor
    {
        private readonly IEnumerable<ISoftwareProvider> softwareInstallationProviders;

        public TestExecutor(IEnumerable<ISoftwareProvider> softwareInstallationProviders)
        {
            this.softwareInstallationProviders = softwareInstallationProviders;
        }

        public Task<bool> TestAsync() =>
            softwareInstallationProviders
                .Select(s => s.GetSoftwarePackagesAsync())
                .Where(s => s != null)
                .SelectMany(s => s)
                .Select(async s => !await s.InitializationPendingAsync() && await s.TestInstalledAsync())
                .AnyAsync();
    }
}
