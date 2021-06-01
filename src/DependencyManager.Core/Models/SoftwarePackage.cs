using DependencyManager.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyManager.Core.Models
{
    public class SoftwarePackage
    {
        protected readonly ISoftwareProvider provider;
        protected readonly IOperatingSystemProvider operatingSystem;

        public IEnumerable<string> Dependencies { get; }
        public string Name { get; }
        public string PackageName { get; }

        public SoftwarePackage(
            KeyValuePair<object, object> yaml,
            ISoftwareProvider provider,
            IOperatingSystemProvider operatingSystem,
            IEnumerable<string> additionalDependencies = null)
        {
            PackageName = yaml.Key as string;
            var dict = yaml.Value as Dictionary<object, object>;

            if (dict != null)
            {
                if (dict.ContainsKey("name"))
                {
                    Name = dict["name"] as string;
                }

                if (dict.ContainsKey("dependencies"))
                {
                    Dependencies = (from d in dict["dependencies"] as List<object>
                                    select d as string).ToList();                }
            }

            this.provider = provider;
            this.operatingSystem = operatingSystem;
        }

        public SoftwarePackage(
            string packageName,
            IEnumerable<string> dependencies,
            ISoftwareProvider provider,
            IOperatingSystemProvider operatingSystem,
            string name = null)
        {
            PackageName = packageName;
            Dependencies = dependencies;
            Name = name;
            this.provider = provider;
            this.operatingSystem = operatingSystem;
        }

        public virtual async Task InstallAsync()
        {
            await InitializeAsync();
            await provider.InstallPackageAsync(this);
        }

        public virtual async Task<bool> TestInstalledAsync()
        {
            await InitializeAsync();
            return await provider.TestPackageInstalledAsync(this);
        }

        private async Task InitializeAsync()
        {
            if (await provider.InitializationPendingAsync())
            {
                if (provider.InstallRequiresAdmin && !await operatingSystem.IsUserAdminAsync())
                {
                    throw new AdministratorRequiredException();
                }

                await provider.InitializeAsync();
            }
        }
    }

    public class SoftwarePackage<T> : SoftwarePackage
    {
        public T Data { get; }

        public SoftwarePackage(
            KeyValuePair<object, object> yaml,
            T data,
            ISoftwareProvider provider,
            IOperatingSystemProvider operatingSystemProvider)
            : base(yaml, provider, operatingSystemProvider)
        {
            Data = data;
        }

        public SoftwarePackage(
            string packageName,
            IEnumerable<string> dependencies,
            T data,
            ISoftwareProvider provider,
            IOperatingSystemProvider operatingSystemProvider,
            string name = null)
            : base(packageName, dependencies, provider, operatingSystemProvider, name)
        {
            Data = data;
        }
    }
}
