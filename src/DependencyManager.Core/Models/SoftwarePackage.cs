using DependencyManager.Core.Providers;

namespace DependencyManager.Core.Models
{
    public class SoftwarePackage
    {
        protected readonly ISoftwareProvider provider;
        protected readonly IOperatingSystemProvider operatingSystem;

        public IEnumerable<string> Dependencies { get; }
        public string? Name { get; }
        public string? Id { get; }
        public string PackageName { get; }

        public SoftwarePackage(
            KeyValuePair<object, object> yaml,
            ISoftwareProvider provider,
            IOperatingSystemProvider operatingSystem)
        {
            if (yaml.Key is string key)
            {
                PackageName = key;
            }
            else
            {
                PackageName = string.Empty;
                throw new Exception("YAML key is not a string");
            }

            if (yaml.Value is Dictionary<object, object> dict)
            {
                if (dict.ContainsKey("name") && dict["name"] is string name)
                {
                    Name = name;
                }

                if (dict.ContainsKey("id") && dict["id"] is string id)
                {
                    Id = id;
                }

                if (dict.ContainsKey("dependencies"))
                {
                    Dependencies = (from d in dict["dependencies"] as List<object>
                                    select d as string).ToList();
                }
            }

            this.provider = provider;
            this.operatingSystem = operatingSystem;

            if (Dependencies == null)
            {
                Dependencies = Enumerable.Empty<string>();
            }
        }

        public SoftwarePackage(
            string packageName,
            IEnumerable<string> dependencies,
            ISoftwareProvider provider,
            IOperatingSystemProvider operatingSystem,
            string? name = null,
            string? id = null)
        {
            PackageName = packageName;
            Dependencies = dependencies;
            Name = name;
            this.provider = provider;
            this.operatingSystem = operatingSystem;
            Id = id;
        }

        public virtual Task<bool> InitializationPendingAsync() =>
            provider.InitializationPendingAsync();

        public virtual async Task InstallAsync()
        {
            await InitializeAsync();

            var isUserAdmin = await operatingSystem.IsSuperUserAsync();
            if (provider.RequiredPermissions == PermissionRequirements.SuperUser && !isUserAdmin)
                throw new SuperUserRequiredException();
            else if (provider.RequiredPermissions == PermissionRequirements.User && isUserAdmin)
                throw new UserRequiredException();
            else
                await provider.InstallPackageAsync(this);
        }

        public virtual async Task<bool> TestInstalledAsync()
        {
            await InitializeAsync();
            return await provider.TestPackageInstalledAsync(this);
        }

        public virtual Task<bool> TestPlatformAsync() =>
            provider.TestPlatformAsync();

        private async Task InitializeAsync()
        {
            if (await provider.InitializationPendingAsync())
            {
                var isUserAdmin = await operatingSystem.IsSuperUserAsync();
                if (provider.RequiredPermissions == PermissionRequirements.SuperUser && !isUserAdmin)
                    throw new SuperUserRequiredException();
                else if (provider.RequiredPermissions == PermissionRequirements.User && isUserAdmin)
                    throw new UserRequiredException();
                else
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
            string? name = null)
            : base(packageName, dependencies, provider, operatingSystemProvider, name)
        {
            Data = data;
        }
    }
}
