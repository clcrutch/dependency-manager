using Clcrutch.Extensions.DependencyInjection;
using DependencyManager.Core;
using DependencyManager.Core.Models;
using DependencyManager.Core.Providers;
using Microsoft.Dism;
using Newtonsoft.Json;
using System.Composition;

namespace DependencyManager.Providers.Windows
{
    [Export(typeof(ISoftwareProvider))]
    [OperatingSystemRequired(OperatingSystems.Windows)]
    public class WindowsFeatureSoftwareProvider : SoftwareProviderBase, IDisposable
    {
        private bool disposed = false;
        private DismSession? session;

        public override PermissionRequirements RequiredPermissions => PermissionRequirements.SuperUser;
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
                session?.Close();
                session?.Dispose();
                DismApi.Shutdown();
                disposed = true;

                GC.SuppressFinalize(this);
            }
        }

        public override Task<bool> InitializationPendingAsync() =>
            Task.FromResult(false);

        public override Task InitializeAsync() =>
            Task.CompletedTask;

        public override Task InstallPackageAsync(SoftwarePackage package)
        {
            DismApi.EnableFeatureByPackageName(GetDismSession(), package.PackageName, null, false, true);
            return UpdateCacheAsync(GetCurrentFeatures());
        }

        public override async Task<bool> TestPackageInstalledAsync(SoftwarePackage package)
        {
            var cachedFeatures = await GetCachedFeaturesAsync();

            if (cachedFeatures != null && cachedFeatures[package.PackageName] == DismPackageFeatureState.Installed)
            {
                return true;
            }

            if (!await operatingSystemProvider.IsSuperUserAsync())
            {
                throw new SuperUserRequiredException();
            }

            var features = GetCurrentFeatures();

            return features[package.PackageName] == DismPackageFeatureState.Installed;
        }

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

        private FileInfo GetCacheInfo()
        {
            var cachePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Clcrutch", "DependencyManager", "cache");
            var cacheDirectoryInfo = new DirectoryInfo(cachePath);

            if (!cacheDirectoryInfo.Exists)
            {
                cacheDirectoryInfo.Create();
            }

            var installedPackagesCachePath = Path.Combine(cachePath, "WindowsFeatureState.json");
            return new FileInfo(installedPackagesCachePath);
        }

        private Dictionary<string, DismPackageFeatureState> GetCurrentFeatures() =>
            DismApi.GetFeatures(GetDismSession()).ToDictionary(x => x.FeatureName, x => x.State);

        private async Task<Dictionary<string, DismPackageFeatureState>?> GetCachedFeaturesAsync()
        {
            var cacheInfo = GetCacheInfo();

            if (!cacheInfo.Exists)
            {
                return null;
            }

            using var reader = cacheInfo.OpenText();
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, DismPackageFeatureState>>(await reader.ReadToEndAsync());
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task UpdateCacheAsync(Dictionary<string, DismPackageFeatureState> features)
        {
            var cacheInfo = GetCacheInfo();

            if (cacheInfo.Exists)
            {
                cacheInfo.Delete();
            }

            using var writer = cacheInfo.CreateText();
            await writer.WriteAsync(JsonConvert.SerializeObject(features));
        }
    }
}
