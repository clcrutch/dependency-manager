using DependencyManager.Core.Providers;

namespace DependencyManager.Providers.Default
{
    public class AllPlatformProvider : IPlatformProvider
    {
        public string Name => "all";

        public Task<bool> TestAsync() =>
            Task.FromResult(true);

        public Task<bool> TestAsync(string version) =>
            Task.FromResult(true);
    }
}
