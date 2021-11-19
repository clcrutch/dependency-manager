using DependencyManager.Core.Providers;
using System.Composition;

namespace DependencyManager.Providers.Default
{
    [Export(typeof(IArchitectureProvider))]
    public class Amd64ArchitectureProvider : IArchitectureProvider
    {
        public string Name => "amd64";

        public Task<bool> TestAsync() =>
            Task.FromResult(Environment.Is64BitOperatingSystem);
    }
}
