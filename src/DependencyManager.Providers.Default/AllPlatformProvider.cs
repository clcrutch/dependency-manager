using DependencyManager.Core.Providers;
using System.Composition;

namespace DependencyManager.Providers.Default
{
    [Export(typeof(IPlatformProvider))]
    public class AllPlatformProvider : IPlatformProvider
    {
        public string Name => "all";

        public Task<bool> TestAsync() =>
            Task.FromResult(true);

        public Task<bool> TestAsync(string version) =>
            Task.FromResult(true);
    }
}
