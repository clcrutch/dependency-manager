using DependencyManager.Core.Providers;
using System.Threading.Tasks;

namespace DependencyManager.Providers.Default
{
    public class AllArchitectureProvider : IArchitectureProvider
    {
        public string Name => "All";

        public Task<bool> TestAsync() =>
            Task.FromResult(true);
    }
}
