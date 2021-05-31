using DependencyManager.Core.Models;
using DependencyManager.Core.Providers;
using Microsoft.Dism;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager.Providers.Windows
{
    public class WindowsFeatureSoftwareProvider : SoftwareProviderBase, IDisposable
    {
        private bool disposed = false;
        private DismSession session;

        public override bool InstallRequiresAdmin => true;
        public override bool TestRequiresAdmin => true;
        protected override string SectionName => "feature";

        public WindowsFeatureSoftwareProvider(
            IDependencyConfigurationProvider dependencyConfigurationProvider,
            IOperatingSystemProvider operatingSystemProvider)
            : base(dependencyConfigurationProvider, operatingSystemProvider)
        {
        }

        ~WindowsFeatureSoftwareProvider()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                session.Close();
                session.Dispose();
                DismApi.Shutdown();
                disposed = true;

                GC.SuppressFinalize(this);
            }
        }

        public override Task<bool> InitializationPendingAsync() =>
            Task.FromResult(false);

        public override Task InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public override Task InstallPackageAsync(SoftwarePackage package)
        {
            DismApi.EnableFeatureByPackageName(GetDismSession(), package.PackageName, null, false, true);
            return Task.FromResult(0);
        }

        public override Task<bool> TestPackageInstalledAsync(SoftwarePackage package) =>
            Task.FromResult(DismApi.GetFeatureInfo(GetDismSession(), package.PackageName).FeatureState == DismPackageFeatureState.Installed);

        public override Task<bool> TestPlatformAsync() =>
            Task.FromResult(OperatingSystem.IsWindowsVersionAtLeast(6, 1)); // At least Windows 7.

        private DismSession GetDismSession()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(WindowsFeatureSoftwareProvider));
            }

            if (session == null)
            {
                DismApi.Initialize(DismLogLevel.LogErrors);

                session = DismApi.OpenOnlineSessionEx(new DismSessionOptions
                {
                    ThrowExceptionOnRebootRequired = false
                });
            }

            return session;
        }
    }
}
