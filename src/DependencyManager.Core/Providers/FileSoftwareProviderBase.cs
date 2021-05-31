using DependencyManager.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager.Core.Providers
{
    public abstract class FileSoftwareProviderBase : SoftwareProviderBase
    {
        protected FileSoftwareProviderBase(
            IDependencyConfigurationProvider dependencyConfigurationProvider, 
            IOperatingSystemProvider operatingSystemProvider)
            : base(dependencyConfigurationProvider, operatingSystemProvider)
        {
        }

        protected async Task<FileInfo> GetPackageFileAsync(SoftwarePackage package, string defaultExtension)
        {
            if (IsWebUrl(package))
            {
                var cachePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Clcrutch", "DependencyManager", "cache");
                var cacheDirectoryInfo = new DirectoryInfo(cachePath);

                if (!cacheDirectoryInfo.Exists)
                {
                    cacheDirectoryInfo.Create();
                }

                var filename = Path.GetFileName(new Uri(package.PackageName).LocalPath);
                if (string.IsNullOrEmpty(Path.GetExtension(filename)))
                {
                    filename = $"{filename}{defaultExtension}";
                }
                var filepath = Path.Join(cachePath, filename);

                var fileinfo = new FileInfo(filepath);

                if (!fileinfo.Exists)
                {
                    using var client = new WebClient();
                    await client.DownloadFileTaskAsync(package.PackageName, fileinfo.FullName);
                }

                return fileinfo;
            }
            else
            {
                return new FileInfo(package.PackageName);
            }
        }

        private bool IsWebUrl(SoftwarePackage package) =>
            package.PackageName.Contains("https://") || package.PackageName.Contains("http://");
    }
}
