using DependencyManager.Core;
using DependencyManager.Core.Models;
using DependencyManager.Core.Providers;
using System.Composition;
using System.Diagnostics;

namespace DependencyManager.Providers.Npm
{
    [Export(typeof(ISoftwareProvider))]
    public class NpmSoftwareProvider : SoftwareProviderBase
    {
        public override PermissionRequirements RequiredPermissions => PermissionRequirements.None;
        protected override string SectionName => "npm";

        public NpmSoftwareProvider(
            IDependencyConfigurationProvider dependencyConfigurationProvider,
            IOperatingSystemProvider operatingSystemProvider)
            : base(dependencyConfigurationProvider, operatingSystemProvider)
        {
        }

        public override Task<bool> InitializationPendingAsync() =>
            Task.FromResult(false);

        public override Task InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public override async Task InstallPackageAsync(SoftwarePackage package)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = await operatingSystemProvider.GetFullExecutablePathAsync("npm"),
                Arguments = $"install -g {package.PackageName}"
            });

            if (process == null)
            {
                throw new Exception("Process did not start correctly.");
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new InstallFailedException();
            }
        }

        public override async Task<bool> TestPackageInstalledAsync(SoftwarePackage package)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = await operatingSystemProvider.GetFullExecutablePathAsync("npm"),
                Arguments = $"list -g {package.PackageName}",
                RedirectStandardOutput = true,
            });

            if (process == null)
            {
                throw new Exception("Process did not start correctly.");
            }

            process.WaitForExit();
            return process.ExitCode == 0;
        }

        public override async Task<bool> TestPlatformAsync() =>
            !string.IsNullOrEmpty(await operatingSystemProvider.GetFullExecutablePathAsync("npm"));
    }
}