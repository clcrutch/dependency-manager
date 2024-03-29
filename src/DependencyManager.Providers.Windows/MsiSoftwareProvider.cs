﻿using Clcrutch.Extensions.DependencyInjection;
using DependencyManager.Core;
using DependencyManager.Core.Models;
using DependencyManager.Core.Providers;
using Microsoft.Win32;
using System.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace DependencyManager.Providers.Windows
{
    [Export(typeof(ISoftwareProvider))]
    [OperatingSystemRequired(OperatingSystems.Windows)]
    public class MsiSoftwareProvider : FileSoftwareProviderBase
    {
        public override PermissionRequirements RequiredPermissions => PermissionRequirements.SuperUser;
        protected override string SectionName => "msi";

        public MsiSoftwareProvider(
            IDependencyConfigurationProvider dependencyConfigurationProvider,
            IOperatingSystemProvider operatingSystemProvider)
            : base(dependencyConfigurationProvider, operatingSystemProvider)
        {
        }

        public override Task<bool> InitializationPendingAsync() =>
            Task.FromResult(false);

        public override Task InitializeAsync() =>
            Task.CompletedTask;

        public override async Task InstallPackageAsync(SoftwarePackage package)
        {
            var fileinfo = await GetPackageFileAsync(package, ".msi");

            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "msiexec",
                Arguments = $"/i {fileinfo.Name} /quiet /qn /norestart",
                WorkingDirectory = fileinfo.DirectoryName
            });
            await (process?.WaitForExitAsync() ?? Task.CompletedTask);

            if (process == null || process.ExitCode != 0)
            {
                throw new InstallFailedException();
            }
        }

        public override async Task<bool> TestPackageInstalledAsync(SoftwarePackage package)
        {
            var fileinfo = await GetPackageFileAsync(package, ".msi");
            var productName = GetProductName(fileinfo.FullName);

            if (OperatingSystem.IsWindows())
            {
                var productsKey = Registry.ClassesRoot.OpenSubKey($"Installer\\Products");
                return (from s in productsKey?.GetSubKeyNames()
                        where OperatingSystem.IsWindows() &&
                            productsKey?.OpenSubKey(s)?.GetValue("ProductName")?.ToString() == productName
                        select s).Any();
            }

            throw new NotImplementedException();
        }

        public override Task<bool> TestPlatformAsync() =>
            Task.FromResult(OperatingSystem.IsWindows());

        [DllImport("msi.dll", CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true, ExactSpelling = true)]
        private static extern UInt32 MsiOpenPackageW(string szPackagePath, out IntPtr hProduct);
        [DllImport("msi.dll", CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true, ExactSpelling = true)]
        private static extern uint MsiCloseHandle(IntPtr hAny);
        [DllImport("msi.dll", CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true, ExactSpelling = true)]
        private static extern uint MsiGetPropertyW(IntPtr hAny, string name, StringBuilder buffer, ref int bufferLength);
        private static string? GetPackageProperty(string msi, string property)
        {
            IntPtr MsiHandle = IntPtr.Zero;
            try
            {
                var res = MsiOpenPackageW(msi, out MsiHandle);
                if (res != 0)
                {
                    return null;
                }
                int length = 256;
                var buffer = new StringBuilder(length);
                res = MsiGetPropertyW(MsiHandle, property, buffer, ref length);
                return buffer.ToString();
            }
            finally
            {
                if (MsiHandle != IntPtr.Zero)
                {
                    MsiCloseHandle(MsiHandle);
                }
            }
        }
        private static string? GetProductCode(string msi)
        {
            return GetPackageProperty(msi, "ProductCode");
        }
        private static string? GetProductName(string msi)
        {
            return GetPackageProperty(msi, "ProductName");
        }
    }
}
