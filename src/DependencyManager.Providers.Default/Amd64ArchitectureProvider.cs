using DependencyManager.Core.Providers;

namespace DependencyManager.Providers.Default
{
    public class Amd64ArchitectureProvider : IArchitectureProvider
    {
        public string Name => "amd64";

        public Task<bool> TestAsync() =>
            Task.FromResult(Environment.Is64BitOperatingSystem);
    }
}
