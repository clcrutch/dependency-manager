using System.Composition;
using System.Diagnostics;
using Clcrutch.Extensions.DependencyInjection;
using DependencyManager.Core;
using DependencyManager.Core.Models;
using DependencyManager.Core.Providers;

namespace DependencyManager.Providers.Linux
{
    [Export(typeof(ISoftwareProvider))]
    [OperatingSystemRequired(OperatingSystems.Linux)]
    public class SnapSoftwareProvider : SoftwareProviderBase
    {
        private const string CLASSIC_CHECK =
            "If you understand and want to proceed repeat the command including --classic.";

        public override PermissionRequirements RequiredPermissions => PermissionRequirements.SuperUser;
        protected override string SectionName => "snap";

        public SnapSoftwareProvider(
            IDependencyConfigurationProvider dependencyConfigurationProvider,
            IOperatingSystemProvider operatingSystemProvider)
            : base(dependencyConfigurationProvider, operatingSystemProvider)
        {
        }
        
        public override Task InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> InitializationPendingAsync() =>
            Task.FromResult(false);

        public override Task InstallPackageAsync(SoftwarePackage package) =>
            InstallPackageInternalAsync(package, false);

        private async Task InstallPackageInternalAsync(SoftwarePackage package, bool classic)
        {
            var arguments = $"install {package.PackageName}";

            if (classic)
            {
                arguments += " --classic";
            }

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "snap",
                Arguments = arguments,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                await process.WaitForExitAsync();
                var results = await process.StandardError.ReadToEndAsync();
                var needsClassic = (from l in results.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                    where l.Contains(CLASSIC_CHECK)
                                    select l).Any();

                if (needsClassic && !classic)
                {
                    await InstallPackageInternalAsync(package, true);
                    return;
                }
            }

            if (process == null || process.ExitCode != 0)
            {
                throw new InstallFailedException();
            }
        }

        public override async Task<bool> TestPackageInstalledAsync(SoftwarePackage package) =>
            (await GetInstalledPackagesAsync())?.ContainsKey(package.PackageName) ?? false;

        public override async Task<bool> TestPlatformAsync() =>
            !string.IsNullOrEmpty(await operatingSystemProvider.GetFullExecutablePathAsync("snap"));
        
        private async Task<Dictionary<string, string>?> GetInstalledPackagesAsync()
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "snap",
                Arguments = "list",
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            if (process != null)
            {
                await process.WaitForExitAsync();
                var packageString = await process.StandardOutput.ReadToEndAsync();
                var packageLines = packageString.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

                return (from l in packageLines.Skip(1)
                        where !string.IsNullOrWhiteSpace(l)
                        select l.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToDictionary(s => s[0], s => s[1]);
            }

            return null;
        }
    }
}