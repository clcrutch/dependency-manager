﻿using DependencyManager.Core.Models;
using DependencyManager.Core.Providers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using DependencyManager.Core;

namespace DependencyManager.Providers.VSCode
{
    public class VSCodeSoftwareProvider : SoftwareProviderBase
    {
        public override bool InstallRequiresAdmin => false;

        public override bool TestRequiresAdmin => false;

        protected override string SectionName => "vscode";

        public VSCodeSoftwareProvider(
            IDependencyConfigurationProvider dependencyConfigurationProvider, 
            IOperatingSystemProvider operatingSystemProvider)
            : base (dependencyConfigurationProvider, operatingSystemProvider)

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
                FileName = await operatingSystemProvider.GetFullExecutablePathAsync("code"),
                Arguments = $"--install-extension {package.PackageName}"
            });

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InstallFailedException();
            }
        }

        public override async Task<bool> TestPackageInstalledAsync(SoftwarePackage package)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = await operatingSystemProvider.GetFullExecutablePathAsync("code"),
                Arguments = "--list-extensions",
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            await process.WaitForExitAsync();
            var packageString = await process.StandardOutput.ReadToEndAsync();
            var packageLines = packageString.Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();

            return packageLines.Contains(package.PackageName);
        }

        public override async Task<bool> TestPlatformAsync() =>
            !string.IsNullOrEmpty(await operatingSystemProvider.GetFullExecutablePathAsync("code"));
    }
}