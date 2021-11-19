using DependencyManager.Core.Models;

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
                var cachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Clcrutch", "DependencyManager", "cache");
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
                var filepath = Path.Combine(cachePath, filename);

                var fileinfo = new FileInfo(filepath);

                if (!fileinfo.Exists)
                {
                    using var client = new HttpClient();
                    var webStream = await client.GetStreamAsync(package.PackageName);
                    var fileStream = fileinfo.Create();

                    await webStream.CopyToAsync(fileStream);
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
