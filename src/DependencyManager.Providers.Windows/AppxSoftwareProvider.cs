using DependencyManager.Core;
using DependencyManager.Core.Models;
using DependencyManager.Core.Providers;
using Microsoft.PowerShell;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DependencyManager.Providers.Windows
{
    public class AppxSoftwareProvider : FileSoftwareProviderBase
    {
        public override bool InstallRequiresAdmin => false;
        protected override string SectionName => "appx";

        public AppxSoftwareProvider(
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

        public async override Task InstallPackageAsync(SoftwarePackage package)
        {
            var fileinfo = await GetPackageFileAsync(package, ".appx");

            var sessionState = InitialSessionState.CreateDefault();
            sessionState.ExecutionPolicy = ExecutionPolicy.Unrestricted;

            using var ps = PowerShell.Create(sessionState);
            ps.AddScript($"cd \"{fileinfo.DirectoryName}\"");
            ps.AddScript("Import-Module Appx -UseWindowsPowerShell");
            ps.AddScript($"Add-AppxPackage '{package.PackageName}'");

            await ps.InvokeAsync();

            if (ps.HadErrors)
            {
                throw new InstallFailedException();
            }
        }

        public async override Task<bool> TestPackageInstalledAsync(SoftwarePackage package)
        {
            var fileinfo = await GetPackageFileAsync(package, ".appx");
            using var stream = ZipFile.OpenRead(fileinfo.FullName).GetEntry("AppxManifest.xml").Open();
            using var reader = new StreamReader(stream);

            XNamespace xdocNamespace = "http://schemas.microsoft.com/appx/manifest/foundation/windows10";

            var xml = await reader.ReadToEndAsync();
            var doc = XDocument.Parse(xml);
            var appxName = doc.Descendants(xdocNamespace + "Identity").First().Attribute("Name").Value;

            var sessionState = InitialSessionState.CreateDefault();
            sessionState.ExecutionPolicy = ExecutionPolicy.Unrestricted;

            using var ps = PowerShell.Create(sessionState);
            ps.AddScript("Import-Module Appx -UseWindowsPowerShell");
            ps.AddScript($"Get-AppxPackage '{appxName}'");

            return (await ps.InvokeAsync()).Count > 0;
        }

        public override Task<bool> TestPlatformAsync() =>
            Task.FromResult(OperatingSystem.IsWindowsVersionAtLeast(10));
    }
}
