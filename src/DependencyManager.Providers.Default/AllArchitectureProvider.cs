using DependencyManager.Core.Providers;
using System.Composition;

namespace DependencyManager.Providers.Default
{
    [Export(typeof(IArchitectureProvider))]
    public class AllArchitectureProvider : IArchitectureProvider
    {
        public string Name => "All";

        public Task<bool> TestAsync() =>
            Task.FromResult(true);
    }
}
