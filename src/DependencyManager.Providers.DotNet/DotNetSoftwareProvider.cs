using DependencyManager.Core;
using DependencyManager.Core.Models;
using DependencyManager.Core.Providers;
using System.Composition;
using System.Diagnostics;

namespace DependencyManager.Providers.DotNet
{
    [Export(typeof(ISoftwareProvider))]
    public class DotNetSoftwareProvider : SoftwareProviderBase
    {
        public override PermissionRequirements RequiredPermissions => PermissionRequirements.None;

        protected override string SectionName => "dotnet";

        public DotNetSoftwareProvider(
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
                FileName = await operatingSystemProvider.GetFullExecutablePathAsync("dotnet"),
                Arguments = $"tool install --global {package.PackageName}"
            });

            if (process == null)
            {
                throw new Exception("Process did not start correctly.");
            }

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InstallFailedException();
            }
        }

        public override async Task<bool> TestPackageInstalledAsync(SoftwarePackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            if (package.PackageName == null)
            {
                throw new ArgumentNullException(nameof(package.PackageName));
            }

            // dotnet tool list --global
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = await operatingSystemProvider.GetFullExecutablePathAsync("dotnet"),
                Arguments = $"tool list --global",
                RedirectStandardOutput = true,
            });

            if (process == null)
            {
                throw new Exception("Process did not start correctly.");
            }

            await process.WaitForExitAsync();
            var results = await process.StandardOutput.ReadToEndAsync();

            return results.Contains(package.PackageName, StringComparison.InvariantCultureIgnoreCase);
        }

        public override async Task<bool> TestPlatformAsync() =>
            !string.IsNullOrEmpty(await operatingSystemProvider.GetFullExecutablePathAsync("dotnet"));
    }
}